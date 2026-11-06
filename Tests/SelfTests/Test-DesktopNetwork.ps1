param(
    [Parameter(Mandatory=$true)][string]$ExecutablePath,
    [ValidateSet('Russian','English')][string]$Language='Russian',
    [ValidateSet('Light','Dark')][string]$Theme='Dark'
)
$ErrorActionPreference='Stop'
$caseDirectory=Join-Path $PSScriptRoot ('bin/NetworkPage/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $caseDirectory -Force|Out-Null
$appProcess=$null
$appLog=Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength=if(Test-Path -LiteralPath $appLog){(Get-Item -LiteralPath $appLog).Length}else{0}
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NetworkPageTestNative {
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
}
"@
[void][NetworkPageTestNative]::SetThreadDpiAwarenessContext([IntPtr](-4))

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
 [void][NetworkPageTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
 [void][NetworkPageTestNative]::SetCursorPos(1600,1150)
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
 [void][NetworkPageTestNative]::SetWindowPos($appProcess.MainWindowHandle,[IntPtr](-1),100,80,1200,1000,0x0040)
 Invoke-Element (Find-Element '^(Сетевая активность|Network activity)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 [void](Find-Element '^(Активность|Activity)$')
 $elements=(Get-Window).FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
 if($elements|Where-Object{$_.Current.Name -match '^(Последние операции|Recent operations)$'}){throw 'На странице остался удалённый блок последних операций'}
 [void](Find-Element '.*' 'NetworkEmpty')
 Invoke-Element (Find-Element '.*' 'NetworkClear')
 $refresh=Find-Element '.*' 'ToolbarRefresh';$update=Find-Element '.*' 'ToolbarUpdate'
 if($refresh.Current.Name -eq $update.Current.Name){throw 'Действия страницы и программы не различаются по подписи'}
 $refreshRect=$refresh.Current.BoundingRectangle;$languageRect=(Find-Element '.*' 'ToolbarLanguage').Current.BoundingRectangle
 if($refreshRect.Right -gt $languageRect.Left -or $languageRect.Left-$refreshRect.Right -gt 12){throw 'Обновление страницы не находится перед выбором языка'}
 if((Find-Element '.*' 'NetworkStopParsing' -AllowDisabled).Current.IsEnabled){throw 'Прерывание доступно без парсинга'}
 if((Find-Element '.*' 'NetworkPauseParsing' -AllowDisabled).Current.IsEnabled){throw 'Пауза доступна без парсинга'}
 Save-Snapshot 'network-empty-wide'

 # Загружаем реальную строку прокси обновлением уже открытой страницы
 Invoke-Element (Find-Element '^(Прокси|Proxies)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 $testListener=[Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback,0)
 $testListener.Start(1);$port=$testListener.LocalEndpoint.Port
 & dotnet run --project (Join-Path $PSScriptRoot 'Proxies/Proxies.SelfTest.csproj') -c Debug -p:Platform=x64 --no-build -- `
  --seed-network-proxy (Join-Path $caseDirectory 'TgStorage.db') $port
 if($LASTEXITCODE -ne 0){throw 'Не удалось подготовить тестовую БД'}
 Invoke-Element (Find-Element '.*' 'ToolbarRefresh')
 [void](Find-Element '^127\.0\.0\.1$')
 Write-Output 'PASS: верхняя кнопка действительно перечитывает данные текущей страницы'
 Invoke-Element (Find-Element '^(Проверить все прокси|Check all proxies)$')
 [void](Find-Element '^(Доступен|Available) \(TCP,')
 if(!$testListener.Pending()){throw 'Проверка прокси не создала реальное TCP-подключение'}
 $accepted=$testListener.AcceptTcpClient();$accepted.Dispose()
 Invoke-Element (Find-Element '^(Сетевая активность|Network activity)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 $row=Find-Element '^(Проверка прокси|Proxy check)$'
 if($row.Current.BoundingRectangle.Height -gt 32){throw 'Строка операции не компактная'}
 [void](Find-Element '^(Завершено|Completed)$')
 $elements=(Get-Window).FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
 if($elements|Where-Object{$_.Current.AutomationId -eq 'NetworkEmpty' -and !$_.Current.IsOffscreen}){throw 'История показана вместе с пустым состоянием'}
 Save-Snapshot 'network-history'
 Invoke-Element (Find-Element '.*' 'ToolbarRefresh')
 [void](Find-Element '^(Завершено|Completed)$')
 Write-Output 'PASS: настоящее TCP-подключение завершено, строка видна в группе; обновление не сбрасывает историю'
 # Раскрытие группы должно сохраняться после очередного обновления страницы
 $group=Find-Element '.*' 'NetworkChatGroup'
 $expand=$group.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
 $expand.Collapse();Start-Sleep -Milliseconds 1300
 $expand=(Find-Element '.*' 'NetworkChatGroup').GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
 if($expand.Current.ExpandCollapseState -ne [System.Windows.Automation.ExpandCollapseState]::Collapsed){throw 'Таймер раскрыл свёрнутую группу'}
 $expand.Expand();[void](Find-Element '^(Завершено|Completed)$')
 Save-Snapshot 'network-group-expanded'
 Write-Output 'PASS: группа сворачивается, раскрывается и сохраняет состояние после опроса таймера'
 Invoke-Element (Find-Element '.*' 'NetworkClear')
 [void](Find-Element '^(Завершённых операций: 0 из 100|Finished operations: 0 of 100)$')
 [void](Find-Element '.*' 'NetworkEmpty')
 $transform=(Get-Window).GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
 $transform.Resize(800,900);Start-Sleep -Milliseconds 400
 foreach($id in @('ToolbarRefresh','ToolbarUpdate','ToolbarLanguage','ToolbarTheme','ToggleSwitchShowSensitiveData','NetworkClear','NetworkStopParsing','NetworkPauseParsing')){
  $rect=(Find-Element '.*' $id -AllowDisabled).Current.BoundingRectangle;$window=(Get-Window).Current.BoundingRectangle
  if($rect.Left -lt $window.Left -or $rect.Right -gt $window.Right){throw "Кнопка обрезана в узком окне: $id"}
 }
 Save-Snapshot 'network-empty-narrow'
 [void]$appProcess.CloseMainWindow()
 if(!$appProcess.WaitForExit(15000) -or $appProcess.ExitCode -ne 0){throw 'Desktop не завершился штатно с кодом 0'}
 $newLog=Get-NewLog;$newLog|Set-Content (Join-Path $caseDirectory 'app.log')
 if($newLog -match 'App_UnhandledException|Exception:'){throw 'В журнале появились исключения'}
 if(!$newLog.Contains('App closed')){throw 'Нет штатного завершения в журнале'}
 Write-Output "PASS: очистка истории, управление при ширине 800, закрытие без исключений ($Language, $Theme); $caseDirectory"
} finally {
 if($null -ne $appProcess -and !$appProcess.HasExited){[void]$appProcess.CloseMainWindow();[void]$appProcess.WaitForExit(15000)}
 if($null -ne $testListener){$testListener.Stop()}
}

