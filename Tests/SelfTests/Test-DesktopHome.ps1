param([Parameter(Mandatory=$true)][string]$ExecutablePath)
$ErrorActionPreference='Stop'
$executable=(Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory=Join-Path $PSScriptRoot ('bin/HomeLayout/'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $caseDirectory -Force|Out-Null
$appProcess=$null
$appLog=Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength=if(Test-Path -LiteralPath $appLog){(Get-Item -LiteralPath $appLog).Length}else{0}
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class HomeLayoutTestNative {
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
}
"@
[void][HomeLayoutTestNative]::SetThreadDpiAwarenessContext([IntPtr](-4))

# Ищем элементы только в проверяемом окне с отдельными настройками
function Find-HomeElement([string]$id='',[string]$name='.*') {
 $deadline=[DateTime]::UtcNow.AddSeconds(30)
 do {
  $appProcess.Refresh()
  if($appProcess.HasExited){throw 'Desktop завершился во время проверки главной страницы'}
  if($appProcess.MainWindowHandle -ne [IntPtr]::Zero){
   $window=[System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
   $elements=$window.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
   foreach($element in $elements){
    if(($id -eq '' -or $element.Current.AutomationId -eq $id) -and $element.Current.Name -match $name -and
      !$element.Current.IsOffscreen -and $element.Current.IsEnabled){return $element}
   }
  }
  Start-Sleep -Milliseconds 100
 }while([DateTime]::UtcNow -lt $deadline)
 throw "Не найден элемент главной страницы: $id $name"
}

# Сохраняем настоящее окно без изменения размера изображения
function Save-HomeSnapshot([string]$name) {
 $window=[System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
 $rect=$window.Current.BoundingRectangle
 $bitmap=[Drawing.Bitmap]::new([int]$rect.Width,[int]$rect.Height)
 $graphics=[Drawing.Graphics]::FromImage($bitmap)
 try{$graphics.CopyFromScreen([int]$rect.Left,[int]$rect.Top,0,0,$bitmap.Size);$bitmap.Save((Join-Path $caseDirectory ($name+'.png')))}
 finally{$graphics.Dispose();$bitmap.Dispose()}
}

try {
 if(Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue){throw 'Перед проверкой закройте Desktop'}
 @{AppStorage=(Join-Path $caseDirectory 'Demo.db');AppSession=(Join-Path $caseDirectory 'Demo.session');
   AppLanguage='Russian';AppTheme='Dark';WindowWidth='1200';WindowHeight='1000';WindowX='50';WindowY='30'}|
  ConvertTo-Json|Set-Content (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
 $start=[Diagnostics.ProcessStartInfo]::new($executable)
 $start.UseShellExecute=$false;$start.WorkingDirectory=Split-Path $executable
 $start.Environment['LocalSettingsOptions__ApplicationDataFolder']=$caseDirectory
 $start.Environment['LocalSettingsOptions__LocalSettingsFile']='Settings.json'
 $start.Environment['DOTNET_ENVIRONMENT']='Production'
 $start.Environment['Logging__LogLevel__Default']='Warning'
 $appProcess=[Diagnostics.Process]::Start($start)
 $continue=Find-HomeElement -name '^(Продолжить|Continue)$'
 $invokePattern=$null
 while(!$continue.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern,[ref]$invokePattern)){
  $continue=[System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($continue)
  if($null -eq $continue){throw 'Кнопка «Продолжить» не поддерживает нажатие'}
 }
 $invokePattern.Invoke()
 foreach($width in @(1600,1200,900,1600)){
  [void](Find-HomeElement -id 'HomeChats')
  [void][HomeLayoutTestNative]::SetWindowPos($appProcess.MainWindowHandle,[IntPtr]::Zero,50,30,$width,1000,0x0040)
  [void][HomeLayoutTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
  Start-Sleep -Milliseconds 900
  $first=Find-HomeElement -id 'HomeChats'
  $last=Find-HomeElement -id 'HomeSettings'
  $viewport=[System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($first)
  while($null -ne $viewport -and $viewport.Current.ClassName -ne 'ScrollViewer'){
   $viewport=[System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($viewport)
  }
  if($null -eq $viewport){throw 'Не найдена область прокрутки главной страницы'}
  $bounds=$viewport.Current.BoundingRectangle
  $left=$first.Current.BoundingRectangle.Left
  $right=$last.Current.BoundingRectangle.Right
  $leftGap=$left-$bounds.Left;$rightGap=$bounds.Right-$right
  $delta=[Math]::Abs($leftGap-$rightGap)
  $measurement="width=$width; left=$leftGap; right=$rightGap; delta=$delta"
  Add-Content (Join-Path $caseDirectory 'measurements.log') $measurement -Encoding utf8
  Save-HomeSnapshot "$width"
  if($leftGap -lt 0 -or $rightGap -lt 0 -or $delta -gt 4){throw "Главная страница смещена или обрезана: $measurement"}
  Write-Output "PASS: $measurement"
 }
 [void]$appProcess.CloseMainWindow()
 if(!$appProcess.WaitForExit(15000)){throw 'Desktop не завершился штатно'}
 Write-Output "PASS: центровка и изменение ширины настоящего окна; $caseDirectory"
}
finally {
 if($null -ne $appProcess -and !$appProcess.HasExited){[void]$appProcess.CloseMainWindow();[void]$appProcess.WaitForExit(15000)}
 if(Test-Path -LiteralPath $appLog){
  $stream=[IO.File]::Open($appLog,'Open','Read','ReadWrite')
  try{
   [void]$stream.Seek($originalLength,'Begin');$reader=[IO.StreamReader]::new($stream)
   try{$newLog=$reader.ReadToEnd()}finally{$reader.Dispose()}
  }finally{$stream.Dispose()}
  $newLog|Set-Content (Join-Path $caseDirectory 'app.log') -Encoding utf8
  if($newLog -match 'App_UnhandledException|Exception:|Unhandled exception'){throw 'В журнале Desktop есть исключения'}
 }
}
