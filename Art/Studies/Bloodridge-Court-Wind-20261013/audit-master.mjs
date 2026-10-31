import fs from 'node:fs';
import {Client} from 'file:///C:/Users/jacob/codex-design-lab/runtimes/ks-pixel-kit-20261013/node_modules/@modelcontextprotocol/sdk/dist/esm/client/index.js';
import {StdioClientTransport} from 'file:///C:/Users/jacob/codex-design-lab/runtimes/ks-pixel-kit-20261013/node_modules/@modelcontextprotocol/sdk/dist/esm/client/stdio.js';
const kit='C:/Users/jacob/codex-design-lab/runtimes/ks-pixel-kit-20261013';
const out='C:/Users/jacob/KindredSiege-Clean/Art/Studies/Bloodridge-Court-Wind-20261013';
const c=new Client({name:'ks-burial-court-audit',version:'1.0.0'});
const t=new StdioClientTransport({command:'C:/Program Files/nodejs/node.exe',args:[kit+'/node_modules/@pebbly/aseprite-ai-artist/dist/cli.js','serve','--headless','--aseprite','C:/Program Files (x86)/Steam/steamapps/common/Aseprite/aseprite.exe'],env:{...process.env,ASEPRITE_USER_FOLDER:kit+'/headless-user',ASEPRITE_AI_PROMPTS:'0'},stderr:'pipe'});
try{
 await c.connect(t);
 const ready=await c.callTool({name:'preflight',arguments:{}});
 if(!ready.structuredContent?.ready||ready.structuredContent.mode!=='headless')throw Error('Headless not ready');
 const opened=await c.callTool({name:'sprite_manage',arguments:{op:'open',path:out+'/bloodridge-court-layered.aseprite'}});
 if(opened.isError)throw Error(JSON.stringify(opened));
 const info=await c.callTool({name:'sprite_info',arguments:{includePalette:false}});
 fs.writeFileSync(out+'/master-info.json',JSON.stringify(info.structuredContent,null,2));
 const val=await c.callTool({name:'validate',arguments:{checks:['layers','antialiasing','export_readiness','animation','palette']}},undefined,{timeout:120000});
 fs.writeFileSync(out+'/aseprite-validation.json',JSON.stringify(val,null,2));
 console.log(JSON.stringify(val.structuredContent||val));
}finally{await c.close();}
