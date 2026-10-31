$ErrorActionPreference='Stop'
if(Get-NetTCPConnection -LocalPort 8191 -State Listen -ErrorAction SilentlyContinue){throw 'ArtLab port is in use'}
$taskRoot='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Cult-Rune-Batch-20261014'
$taskArguments=@('main.py','--listen','127.0.0.1','--port','8191','--user-directory','C:/Users/jacob/ComfyUI-Installs/ComfyUI/user-artlab','--output-directory','C:/Users/jacob/ComfyUI-Shared/output/ArtLab','--database-url','sqlite:///C:/Users/jacob/ComfyUI-Installs/ComfyUI/user-artlab/comfyui.db','--preview-method','none','--disable-dynamic-vram','--reserve-vram','1')
$taskProcess=Start-Process -FilePath 'C:/Users/jacob/ComfyUI-Installs/ComfyUI/artlab-env/Scripts/python.exe' -ArgumentList $taskArguments -WorkingDirectory 'C:/Users/jacob/ComfyUI-Installs/ComfyUI/ComfyUI-ArtLab' -WindowStyle Hidden -RedirectStandardOutput "$taskRoot/runtime.out.log" -RedirectStandardError "$taskRoot/runtime.err.log" -PassThru
Write-Output "Temporary batch runtime parent PID: $($taskProcess.Id)"
