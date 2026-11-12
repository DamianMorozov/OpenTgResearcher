param(
    [Parameter(Mandatory=$true)][string]$ExecutablePath,
    [ValidateSet('Russian','English')][string]$Language='Russian',
    [ValidateSet('Light','Dark')][string]$Theme='Dark'
)
$ErrorActionPreference='Stop'
$caseDirectory=Join-Path $PSScriptRoot ('bin/Connection/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $caseDirectory -Force|Out-Null
$appProcess=$null
$appLog=Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength=if(Test-Path -LiteralPath $appLog){(Get-Item -LiteralPath $appLog).Length}else{0}
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class ConnectionTestNative {
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
}
"@
[void][ConnectionTestNative]::SetThreadDpiAwarenessContext([IntPtr](-4))

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
 foreach($type in @([System.Windows.Automation.InvokePattern]::Pattern,[System.Windows.Automation.SelectionItemPattern]::Pattern,[System.Windows.Automation.TogglePattern]::Pattern,[System.Windows.Automation.ExpandCollapsePattern]::Pattern)){
  if($element.TryGetCurrentPattern($type,[ref]$pattern)){
   if($pattern -is [System.Windows.Automation.InvokePattern]){$pattern.Invoke()}
   elseif($pattern -is [System.Windows.Automation.SelectionItemPattern]){$pattern.Select()}
   elseif($pattern -is [System.Windows.Automation.ExpandCollapsePattern]){$pattern.Expand()}
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
 [void][ConnectionTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
 [void][ConnectionTestNative]::SetCursorPos(1600,1150)
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


 # Первый вход нового профиля открывается без сетевого запроса
 Invoke-Element (Find-Element '^(Подключение клиента|Client connection)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 [void](Find-Element '^(Мастер входа|Sign-in wizard)$')
 Save-Snapshot 'wizard-api'
 Invoke-Element (Find-Element '.*' 'ConnectionWizardNext')
 [void](Find-Element '.*' 'ConnectionMessage')
 Save-Snapshot 'wizard-error'
 Write-Output 'PASS: ошибка пустого API отображена'
 $api=Find-Element '.*' 'ConnectionApiId'
 $pattern=$api.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
 $pattern.SetValue('12345')
 $hash=Find-Element '.*' 'ConnectionApiHash'
 $pattern=$hash.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
 $pattern.SetValue('00112233445566778899aabbccddeeff')
 Invoke-Element (Find-Element '.*' 'ConnectionWizardNext')
 [void](Find-Element '.*' 'ConnectionPhone')
 Save-Snapshot 'wizard-phone'
 Invoke-Element (Find-Element '.*' 'ConnectionWizardBack')
 [void](Find-Element '.*' 'ConnectionApiId')
 Invoke-Element (Find-Element '.*' 'ConnectionCloseWizard')
 [void](Find-Element '^(Центр подключения|Connection center)$')
 $currentApi=Find-Element '.*' 'ConnectionApiId'
 if($currentApi.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value -ne '12345'){throw 'Переход в центр потерял API ID'}
 foreach($width in @(1200,800)) {
  [void][ConnectionTestNative]::SetWindowPos($appProcess.MainWindowHandle,[IntPtr]::Zero,50,30,$width,1000,0x0040)
  Start-Sleep -Milliseconds 700
  Save-Snapshot ('center-'+$width)
  $first=Find-Element '.*' 'ConnectionApiId'
  $last=Find-Element '.*' 'ConnectionOpenWizard'
  $bounds=(Get-Window).Current.BoundingRectangle
  foreach($control in @($first,$last)) {
   $rect=$control.Current.BoundingRectangle
   if($rect.Left -lt $bounds.Left -or $rect.Right -gt $bounds.Right -or $rect.Width -lt 80){throw 'Поле или кнопка обрезаны по ширине'}
  }
 }
 Invoke-Element (Find-Element '.*' 'ConnectionOpenWizard')
 [void](Find-Element '.*' 'ConnectionApiId')
 Invoke-Element (Find-Element '.*' 'ConnectionCloseWizard')
 Invoke-Element (Find-Element '.*' 'ToggleSwitchShowSensitiveData')
 $elements=(Get-Window).FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
 foreach($element in $elements) {
  if($element.Current.AutomationId -in @('ConnectionApiId','ConnectionApiHash','ConnectionPhone') -and !$element.Current.IsOffscreen){throw 'Переключатель не скрывает реквизиты'}
 }
 Save-Snapshot 'center-private'
 Invoke-Element (Find-Element '.*' 'ToggleSwitchShowSensitiveData')
 Invoke-Element (Find-Element '^(Главная|Home)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 Invoke-Element (Find-Element '^(Подключение клиента|Client connection)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 [void](Find-Element '^(Центр подключения|Connection center)$')

 # Многострочное предупреждение должно быть обычным текстом, отдельно от короткого заголовка
 [void][ConnectionTestNative]::SetWindowPos($appProcess.MainWindowHandle,[IntPtr]::Zero,50,30,1200,1000,0x0040)
 Invoke-Element (Find-Element '^(Управление сессией|Manage session)$')
 Invoke-Element (Find-Element '^(Сбросить сессию…|Reset session…)$')
 $heading=Find-Element '^(Сбросить сессию Telegram\?|Reset the Telegram session\?)$' '' 'TextBlock'
 $body=Find-Element '(API-параметры|API credentials)' '' 'TextBlock'
 if($body.Current.Name -notmatch '(пароль\.|if enabled\.)$'){throw 'Последний абзац предупреждения обрезан'}
 $headingRect=$heading.Current.BoundingRectangle
 $bodyRect=$body.Current.BoundingRectangle
 if($bodyRect.Top -lt $headingRect.Bottom -or $bodyRect.Height -lt 60){throw 'Пояснение пересекается с заголовком или не переносится'}
 Save-Snapshot 'session-reset-dialog'
 Invoke-Element (Find-Element '^(Отмена|Cancel)$')
 Write-Output 'PASS: короткий заголовок и перенос последней строки окна сброса, отмена без изменения сессии'
 $savedSettings=Get-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Raw|ConvertFrom-Json
 if(Test-Path -LiteralPath $savedSettings.AppSession){throw 'Локальные переходы создали сессию Telegram'}
 [void]$appProcess.CloseMainWindow()
 if(!$appProcess.WaitForExit(15000) -or $appProcess.ExitCode -ne 0){throw 'Desktop не завершился штатно'}
 $newLog=Get-NewLog
 $newLog|Set-Content (Join-Path $caseDirectory 'app.log') -Encoding utf8
 if($newLog -match 'App_UnhandledException|Exception:|Unhandled exception'){throw 'В журнале есть исключения'}
 if(!$newLog.Contains('App closed')){throw 'Нет записи штатного завершения'}
 Write-Output "PASS: $Language/$Theme; автоматический мастер, API-валидация, телефон, возврат, центр, ручной мастер, приватность, ширины 1200/800, закрытие; $caseDirectory"
 Write-Output 'Граница проверки: Telegram не вызывается; запросы кода и пароля проверяются селфтестами сервиса и VM'
}
finally {
 if($null -ne $appProcess -and !$appProcess.HasExited){[void]$appProcess.CloseMainWindow();[void]$appProcess.WaitForExit(15000)}
}
