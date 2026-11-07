param(
    [Parameter(Mandatory=$true)][string]$ExecutablePath,
    [ValidateSet('Russian','English')][string]$Language='Russian',
    [ValidateSet('Light','Dark')][string]$Theme='Dark'
)
$ErrorActionPreference='Stop'
$caseDirectory=Join-Path $PSScriptRoot ('bin/Pages/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $caseDirectory -Force|Out-Null
$appProcess=$null
$appLog=Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength=if(Test-Path -LiteralPath $appLog){(Get-Item -LiteralPath $appLog).Length}else{0}
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class PageAvailabilityTestNative {
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
}
"@
[void][PageAvailabilityTestNative]::SetThreadDpiAwarenessContext([IntPtr](-4))

# Читаем дерево только проверяемого окна
function Get-Window {
 $appProcess.Refresh()
 if($appProcess.HasExited){throw 'Desktop завершился во время проверки'}
 return [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
}

# Ожидаем доступный элемент по имени или идентификатору
function Find-Element([string]$name='.*',[string]$id='', [string]$class='', [switch]$AllowDisabled) {
 $deadline=[DateTime]::UtcNow.AddSeconds(25)
 do {
  $appProcess.Refresh()
  if($appProcess.HasExited){throw "Desktop завершился до появления $name $id"}
  if($appProcess.MainWindowHandle -ne [IntPtr]::Zero){
   try{$elements=(Get-Window).FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)}
   catch{
    $appProcess.Refresh()
    if($appProcess.HasExited){throw 'Desktop завершился во время обновления дерева интерфейса'}
    # WinUI временно перестраивает дерево при смене страницы; постоянная ошибка завершится тайм-аутом
    $_.Exception.ToString()|Add-Content (Join-Path $caseDirectory 'uia-retries.log')
    Start-Sleep -Milliseconds 100
    continue
   }
   foreach($element in $elements){
    if($element.Current.Name -match $name -and ($id -eq '' -or $element.Current.AutomationId -eq $id) -and
      ($class -eq '' -or $element.Current.ClassName -eq $class) -and !$element.Current.IsOffscreen -and ($AllowDisabled -or $element.Current.IsEnabled)){return $element}
   }
  }
  Start-Sleep -Milliseconds 100
 }while([DateTime]::UtcNow -lt $deadline)
 $elements|ForEach-Object{"$($_.Current.ClassName) | $($_.Current.Name) | $($_.Current.AutomationId)"}|Set-Content (Join-Path $caseDirectory 'failure-ui.txt')
 throw "Нет элемента: $name $id"
}

# Вызываем штатную команду или выбираем вкладку
function Invoke-Element($element) {
 $pattern=$null
 foreach($type in @([System.Windows.Automation.InvokePattern]::Pattern,[System.Windows.Automation.SelectionItemPattern]::Pattern,[System.Windows.Automation.TogglePattern]::Pattern)){
  if($element.TryGetCurrentPattern($type,[ref]$pattern)){
   if($pattern -is [System.Windows.Automation.InvokePattern]){$pattern.Invoke()}
   elseif($pattern -is [System.Windows.Automation.SelectionItemPattern]){$pattern.Select()}
   else{$pattern.Toggle()}
   return
  }
 }
 $parent=[System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($element)
 if($null -eq $parent){throw 'Элемент не поддерживает действие'}
 Invoke-Element $parent
}

# Сохраняем настоящее окно без рабочего стола
function Save-Snapshot([string]$name) {
 [void][PageAvailabilityTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
 [void][PageAvailabilityTestNative]::SetCursorPos(1600,1150)
 Start-Sleep -Milliseconds 350
 $rect=(Get-Window).Current.BoundingRectangle
 $bitmap=[Drawing.Bitmap]::new([int]$rect.Width,[int]$rect.Height)
 $graphics=[Drawing.Graphics]::FromImage($bitmap)
 try{$graphics.CopyFromScreen([int]$rect.Left,[int]$rect.Top,0,0,$bitmap.Size);$bitmap.Save((Join-Path $caseDirectory ($name+'.png')))}
 finally{$graphics.Dispose();$bitmap.Dispose()}
}

# Читаем журнал только этого запуска
function Get-NewLog {
 if(!(Test-Path -LiteralPath $appLog)){return ''}
 $stream=[IO.File]::Open($appLog,'Open','Read','ReadWrite')
 try{
  [void]$stream.Seek($originalLength,'Begin');$reader=[IO.StreamReader]::new($stream)
  try{return $reader.ReadToEnd()}finally{$reader.Dispose()}
 }finally{$stream.Dispose()}
}

try {
 if(Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue){throw 'Перед проверкой закройте Desktop'}
 $settings=@{AppStorage=(Join-Path $caseDirectory 'TgStorage.db');AppSession=(Join-Path $caseDirectory 'OpenTgResearcher.session');
  AppLanguage=$Language;AppTheme=$Theme;WindowWidth='1200';WindowHeight='1000';WindowX='100';WindowY='80'}
 $settings|ConvertTo-Json|Set-Content (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
 $start=[Diagnostics.ProcessStartInfo]::new((Resolve-Path -LiteralPath $ExecutablePath).Path)
 $start.UseShellExecute=$false;$start.WorkingDirectory=Split-Path $start.FileName
 $start.Environment['LocalSettingsOptions__ApplicationDataFolder']=$caseDirectory
 $start.Environment['LocalSettingsOptions__LocalSettingsFile']='Settings.json'
 $start.Environment['Logging__LogLevel__Default']='Warning'
 $start.Environment['DOTNET_ENVIRONMENT']='Production'
 $appProcess=[Diagnostics.Process]::Start($start)
 Invoke-Element (Find-Element '^(Продолжить|Continue)$')

 # Проверяем настоящее окно без авторизации и пользовательской БД
 foreach($case in @(
  @('^(Хранилище|Storage)$','^(Таблицы хранилища|Storage tables)$','storage'),
  @('^(Подключение клиента|Client connection)$','^(АПИ ИД|API ID)$','connection'),
  @('^(Фильтры|Filters)$','^(Загрузка хранилища|Loading storage)$','filters'),
  @('^(Сторис|Stories)$','^(Загрузка хранилища|Loading storage)$','stories'),
  @('^(Пользователи и контакты|Users and contacts)$','^(Загрузить ещё|Load more)$','users')
 )) {
  Invoke-Element (Find-Element $case[0] '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
  [void](Find-Element $case[1])
  if($case[2] -eq 'storage') {
   $tabs=@(
    @('^(Таблицы хранилища|Storage tables)$','.*','DataGridStorage'),
    @('^(Настройка хранилища|Storage configuration)$','^(Создать бэкап|Backup storage)$',''),
    @('^(Расширенные настройки хранения|Advanced storage settings)$','^(Сброс поля автоматического обновления|Reset auto update field)$','')
   )
   for($index=0;$index -lt $tabs.Count;$index++) {
    Invoke-Element (Find-Element $tabs[$index][0])
    [void](Find-Element $tabs[$index][1] $tabs[$index][2])
    Save-Snapshot ('storage-'+$index)
   }
  }
  $gridIds=@{filters='DataGridFilters';stories='DataGridStories';users='DataGridUsers'}
  if($gridIds.ContainsKey($case[2])){[void](Find-Element '.*' $gridIds[$case[2]])}
  Save-Snapshot $case[2]
  Write-Output ('PASS: открыта страница '+$case[2])
 }
 [void]$appProcess.CloseMainWindow()
 if(!$appProcess.WaitForExit(15000) -or $appProcess.ExitCode -ne 0){throw 'Desktop не завершился штатно'}
 $newLog=Get-NewLog
 $newLog|Set-Content (Join-Path $caseDirectory 'app.log') -Encoding utf8
 if($newLog -match 'App_UnhandledException|Exception:|Unhandled exception'){throw 'В журнале есть исключения'}
 if(!$newLog.Contains('App closed')){throw 'Нет записи штатного завершения'}
 Write-Output "PASS: пять разделов и три вкладки хранилища, закрытие с кодом 0, журнал без исключений; $caseDirectory"
 Write-Output 'Граница проверки: Telegram не подключён; занятые контексты и состояние парсинга проверяются селфтестами моделей'
}
finally {
 if($null -ne $appProcess -and !$appProcess.HasExited){[void]$appProcess.CloseMainWindow();[void]$appProcess.WaitForExit(15000)}
}
