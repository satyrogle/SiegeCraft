local p=app.params
local src=app.open(p["in"]);app.activeSprite=src
src:setPalette(Palette{fromFile=p["palette"]})
app.command.ChangePixelFormat{format="indexed",dithering="none"}
src:saveCopyAs(p["out"]);src:close()
