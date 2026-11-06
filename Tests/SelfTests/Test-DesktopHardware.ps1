param(
    [Parameter(Mandatory=$true)][string]$ExecutablePath,
    [ValidateSet('Russian','English')][string]$Language='Russian',
    [ValidateSet('Light','Dark')][string]$Theme='Dark'
)
$ErrorActionPreference='Stop'
$caseDirectory=Join-Path $PSScriptRoot ('bin/HardwarePage/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $caseDirectory -Force|Out-Null
$appProcess=$null
$appLog=Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength=if(Test-Path -LiteralPath $appLog){(Get-Item -LiteralPath $appLog).Length}else{0}
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class HardwareTestNative {
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
}
"@
[void][HardwareTestNative]::SetThreadDpiAwarenessContext([IntPtr](-4))

# Читаем дерево только проверяемого окна
function Get-Window {
 $appProcess.Refresh()
 if($appProcess.HasExited){throw 'Desktop завершился во время проверки'}
 return [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
}

# Ожидаем доступный элемент по имени или идентификатору
function Find-Element([string]$name='.*',[string]$id='', [string]$class='') {
 $deadline=[DateTime]::UtcNow.AddSeconds(25)
 do {
  $appProcess.Refresh()
  if($appProcess.HasExited){throw "Desktop завершился до появления $name $id"}
  if($appProcess.MainWindowHandle -ne [IntPtr]::Zero){
   $elements=(Get-Window).FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
   foreach($element in $elements){
    if($element.Current.Name -match $name -and ($id -eq '' -or $element.Current.AutomationId -eq $id) -and
      ($class -eq '' -or $element.Current.ClassName -eq $class) -and !$element.Current.IsOffscreen -and $element.Current.IsEnabled){return $element}
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
 [void][HardwareTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
 [void][HardwareTestNative]::SetCursorPos(1600,1150)
 Start-Sleep -Milliseconds 350
 $rect=(Get-Window).Current.BoundingRectangle
 $bitmap=[Drawing.Bitmap]::new([int]$rect.Width,[int]$rect.Height)
 $graphics=[Drawing.Graphics]::FromImage($bitmap)
 try{$graphics.CopyFromScreen([int]$rect.Left,[int]$rect.Top,0,0,$bitmap.Size);$bitmap.Save((Join-Path $caseDirectory ($name+'.png')))}
 finally{$graphics.Dispose();$bitmap.Dispose()}
}

# Сравниваем фон панели и страницы на настоящем кадре окна
function Test-ThemePixels {
 $bitmap=[Drawing.Bitmap]::new((Join-Path $caseDirectory 'overview-wide.png'))
 try{
  $menu=$bitmap.GetPixel(40,$bitmap.Height-300);$page=$bitmap.GetPixel(70,$bitmap.Height-300)
  if([Math]::Abs($menu.R-$page.R)+[Math]::Abs($menu.G-$page.G)+[Math]::Abs($menu.B-$page.B) -gt 30){
   throw 'Фон меню отличается от страницы в выбранной теме'
  }
 }finally{$bitmap.Dispose()}
}

# Подпись шкалы должна оставаться контрастной в обеих темах
function Test-ChartContrast {
 $rect=(Find-Element '.*' 'CpuHistory').Current.BoundingRectangle
 $bitmap=[Drawing.Bitmap]::new(36,20);$graphics=[Drawing.Graphics]::FromImage($bitmap)
 try{
  $graphics.CopyFromScreen([int]$rect.Left,[int]$rect.Top,0,0,$bitmap.Size)
  $pixels=0
  for($x=0;$x -lt $bitmap.Width;$x++){for($y=0;$y -lt $bitmap.Height;$y++){
   $color=$bitmap.GetPixel($x,$y);$brightness=($color.R+$color.G+$color.B)/3
   if(($Theme -eq 'Light' -and $brightness -lt 140) -or ($Theme -eq 'Dark' -and $brightness -gt 150)){$pixels++}
  }}
  if($pixels -lt 15){throw 'Подпись шкалы графика не контрастирует с фоном'}
 }finally{$graphics.Dispose();$bitmap.Dispose()}
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
 [void][HardwareTestNative]::SetWindowPos($appProcess.MainWindowHandle,[IntPtr](-1),100,80,1200,1000,0x0040)
 $hardware=Find-Element '^(Оборудование|Equipment)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem'
 Save-Snapshot 'initial-menu'
 if($hardware.Current.BoundingRectangle.Width -gt 80){throw 'Меню развернулось по умолчанию'}
 Invoke-Element $hardware
 $overview=Find-Element '^(Обзор ресурсов|Resource overview)$' '' 'Microsoft.UI.Xaml.Controls.SelectorBarItem'
 if(!$overview.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Current.IsSelected){throw 'Начальный вид не является обзором'}
 [void](Find-Element '^(Процессор|CPU)$')
 $deadline=[DateTime]::UtcNow.AddSeconds(15)
 do{$reading=(Find-Element '.*' 'HardwareLastReading').Current.Name;if($reading -ne '—'){break};Start-Sleep -Milliseconds 100}while([DateTime]::UtcNow -lt $deadline)
 if($reading -eq '—'){throw 'На странице нет настоящих измерений'}
 Save-Snapshot 'overview-wide'
 Invoke-Element (Find-Element '.*' 'HardwarePause')
 $frozen=(Find-Element '.*' 'HardwareLastReading').Current.Name
 Start-Sleep -Milliseconds 2200
 if((Find-Element '.*' 'HardwareLastReading').Current.Name -ne $frozen){throw 'Пауза не сохранила последние показания'}
 Invoke-Element (Find-Element '^(Динамика нагрузки|Load history)$' '' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
 [void](Find-Element '.*' 'CpuHistory');[void](Find-Element '.*' 'MemoryHistory')
 if((Find-Element '.*' 'HardwareLastReading').Current.Name -ne $frozen){throw 'Смена вида сбросила измерение'}
 Save-Snapshot 'history-paused'
 Invoke-Element (Find-Element '.*' 'HardwarePause')
 $deadline=[DateTime]::UtcNow.AddSeconds(8)
 do{$newReading=(Find-Element '.*' 'HardwareLastReading').Current.Name;if($newReading -ne $frozen){break};Start-Sleep -Milliseconds 150}while([DateTime]::UtcNow -lt $deadline)
 if($newReading -eq $frozen){throw 'Продолжение не возобновило получение данных'}
 Save-Snapshot 'history-wide'
 Test-ChartContrast
 Test-ThemePixels
 Write-Output 'PASS: реальный мониторинг, стартовый обзор, переключение истории, пауза и продолжение'
 $toggle=Find-Element '.*' 'ToggleSwitchShowSensitiveData'
 $toggle.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
 $shown=$toggle.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On
 if($toggle.Current.Name -notmatch '^(Показать|Скрыть|Show|Hide)'){throw 'У глаза нет локализованного действия'}
 Invoke-Element (Find-Element '^(Настройки|Settings)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 $toggle=Find-Element '.*' 'ToggleSwitchShowSensitiveData'
 if(($toggle.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On) -ne $shown){throw 'Состояние глаза сбросилось при навигации'}
 Invoke-Element (Find-Element '^(Оборудование|Equipment)$' '' 'Microsoft.UI.Xaml.Controls.NavigationViewItem')
 $transform=(Get-Window).GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
 $transform.Resize(800,900);Start-Sleep -Milliseconds 600
 Save-Snapshot 'history-narrow'
 Invoke-Element (Find-Element '^(Обзор ресурсов|Resource overview)$' '' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
 Save-Snapshot 'overview-narrow'
 foreach($id in @('HardwarePause','OverviewTab','HistoryTab','ToggleSwitchShowSensitiveData','ToolbarLanguage','ToolbarTheme','ToolbarUpdate')){
  $element=Find-Element '.*' $id;$rect=$element.Current.BoundingRectangle;$windowRect=(Get-Window).Current.BoundingRectangle
  if($rect.Left -lt $windowRect.Left -or $rect.Right -gt $windowRect.Right){throw "Элемент обрезан при ширине 800: $id"}
 }
 Write-Output 'PASS: глаз сохраняет состояние при навигации, подписи и управление доступны при ширине 800'
 if(!(Get-NewLog).Contains('TgHardwareResourceViewModel | Page loaded')){throw 'Нет журнала открытия оборудования'}
 [void]$appProcess.CloseMainWindow()
 if(!$appProcess.WaitForExit(15000)){throw 'Desktop не завершился штатно'}
 $newLog=Get-NewLog
 $newLog|Set-Content (Join-Path $caseDirectory 'app.log')
 if($newLog -match 'App_UnhandledException|Exception:'){throw 'В журнале появились исключения'}
 if(!$newLog.Contains('App closed')){throw 'Нет штатного завершения в журнале'}
 Write-Output "PASS: штатное завершение без исключений; $caseDirectory"
} finally {
 if($null -ne $appProcess -and !$appProcess.HasExited){[void]$appProcess.CloseMainWindow();[void]$appProcess.WaitForExit(15000)}
}
