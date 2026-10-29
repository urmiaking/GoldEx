$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$web=Join-Path $root 'src/App/Server/GoldEx.Server/wwwroot'
$page=Join-Path $web '_viewer272-layout-qa.html'
$vendor=(Join-Path $root 'src/App/Server/GoldEx.Server/obj/Release/net10.0/PubTmp/Out/wwwroot/_content/DevExpress.Blazor.Reporting.JSBasedControls.Common/dx-blazor-reporting.css').Replace('\','/')
$base=(Join-Path $web 'css/devexpress-goldex.css').Replace('\','/')
$responsive=(Join-Path $web 'css/devexpress-viewer-responsive.css').Replace('\','/')
$fix=(Join-Path $web 'css/devexpress-viewer-fixes.css').Replace('\','/')
$html=@"
<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<link rel="stylesheet" href="file:///$vendor"><link rel="stylesheet" href="file:///$base">
<link rel="stylesheet" href="file:///$responsive"><link rel="stylesheet" href="file:///$fix"></head>
<body style="margin:0;--mud-palette-surface:#fff;--mud-palette-background:#f3f1ee;--mud-palette-text-primary:#262626;--mud-palette-text-secondary:#777;--mud-palette-lines-default:#ddd;--mud-palette-primary:#a4760d;--mud-palette-primary-text:#fff">
<div class="gex-report-viewer" style="height:240px">
<div class="dx-blazor-reporting"><div class="dx-designer-viewport">
<div class="dxrd-preview dxrd-designer-wrapper" style="height:160px;position:relative">
<div class="dxrd-toolbar-wrapper" id="wrap"><div class="dxrd-toolbar" id="bar">
<div class="dxrd-toolbar-item" id="first" style="width:65px">Print</div>
<div class="dxrd-toolbar-item" style="width:65px">Export</div>
<div class="dxrd-toolbar-item" style="width:65px">Search</div>
<div class="dxrd-toolbar-item" style="width:65px">Full</div>
<div class="dxrd-toolbar-item" style="width:65px">Zoom In</div>
<div class="dxrd-toolbar-item" style="width:65px">Zoom Out</div>
<div class="dxrd-toolbar-item-zoom" style="width:128px">Whole Page</div>
<div class="dxrd-toolbar-item" style="width:60px">Previous</div>
<div class="dxrd-preview-pager dxrd-toolbar-item" id="pager">
<div class="dxrd-preview-pager-selectbox"><div class="dx-texteditor">1 از 1</div></div></div>
<div class="dxrd-toolbar-item" style="width:60px">Next</div>
</div></div>
<div class="dxrd-print-dialog dx-popup-wrapper"><div class="dx-overlay-content" id="print">Printing</div></div>
</div></div></div></div>
<output id="out"></output>
<script>setTimeout(()=>{
let w=document.querySelector('#wrap'),p=document.querySelector('#pager'),f=document.querySelector('#first'),b=document.querySelector('#bar'),pr=document.querySelector('#print');
let q=x=>x.getBoundingClientRect(); w.scrollLeft=130;
document.querySelector('#out').textContent=JSON.stringify({
 width:innerWidth,wrapperHeight:w.clientHeight,toolbarHeight:b.getBoundingClientRect().height,
 pagerTop:q(p).top,firstTop:q(f).top,pagerBottom:q(p).bottom,toolbarBottom:q(b).bottom,
 sameRow:Math.abs(q(p).top-q(f).top)<30,pagerWithin:q(p).bottom<=q(b).bottom+1,
 scrollWidth:w.scrollWidth,clientWidth:w.clientWidth,scrollLeft:w.scrollLeft,
 noWrap:getComputedStyle(b).flexWrap,printAnimation:getComputedStyle(pr).animationName
});},130)</script></body></html>
"@
[IO.File]::WriteAllText($page,$html,[Text.UTF8Encoding]::new($false))
try{
 $chrome='C:\Program Files\Google\Chrome\Application\chrome.exe'
 $url='file:///'+$page.Replace('\','/')
 foreach($width in @(460,1280,1720)){
  $dom=& $chrome --headless=new --disable-gpu --disable-extensions --window-size="$width,900" --virtual-time-budget=850 --dump-dom $url 2>$null | Select-String 'id="out"'
  if(!$dom -or $dom.Line -notmatch '<output id="out">([^<]+)</output>'){throw 'Chrome layout fixture unavailable'}
  $v=[System.Net.WebUtility]::HtmlDecode($Matches[1]) | ConvertFrom-Json
  Write-Output ('LAYOUT_'+$width+'='+($v|ConvertTo-Json -Compress))
  if($v.noWrap -ne 'nowrap' -or !$v.sameRow -or !$v.pagerWithin -or $v.toolbarHeight -gt 65){throw 'Viewer toolbar wrapped or pager overflowed'}
  if($width -eq 460 -and ($v.scrollWidth -le $v.clientWidth -or $v.scrollLeft -le 0)){throw 'Mobile horizontal scroll regressed'}
  if($v.printAnimation -ne 'gex-report-print-enter'){throw 'DevExpress print-dialog animation not applied'}
 }
} finally {Remove-Item $page -Force -ErrorAction SilentlyContinue}
