using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Fixed saved encounters and an existing archive only; no pilots, seed search or tuning.
        IEnumerator ValidateSpecialArt()
        {
            string fixtures=Argument("-ksSpecialArtValidate");
            string[] keys={"bloodridge_champion_sw","skullpeak_champion_sw","bloodridge_shaman_sw"};
            string[] saves={"raiders-duel.txt","skullpeak-duel.txt","shaman-history.txt"};
            int[] anchors={78,81,72};
            for(int scenario=0;scenario<keys.Length;scenario++)
            {
                string key=keys[scenario];bool replay=scenario==2;
                _playback=null;_fight=null;_pilotOn=false;
                _savePath=Path.Combine(_capture,key+"-save.txt");
                _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,saves[scenario])));
                string archiveBefore=CampaignSave.Write(_campaign);
                if(replay)
                {
                    SetPage(Page.ChooseTribe);
                    var record=_campaign.Archive.Fights.First(f=>f.FightId=="camp.3.raiders.trial");
                    WatchFight(record);
                }
                else
                {
                    SetPage(Page.Offers);yield return null;
                    int offer=-1;
                    for(int i=0;i<_campaign.Offers.Count;i++)if(_campaign.Offers[i].Kind==EncounterKind.Duel){offer=i;break;}
                    Check(offer>=0&&Click("offer-"+offer),key+" duel offered and selected through UI");yield return null;
                    QaChooseDuelist();yield return null;
                    Check(Click("start-fight"),key+" starts through UI");
                }
                _pilotOn=false;_paused=true;_speed=1;yield return null;
                string suffix=replay?".shaman":".champion";
                var special=_figures.First(f=>f.unit.Side==1&&f.unit.Id.EndsWith(suffix,StringComparison.Ordinal));
                Check(special.artKey==key&&special.body!=null,key+" replaces its pending token");
                int size=replay?156:180,anchorY=replay?132:152;
                var clips=new[]{special.idle,special.attack,special.hit,special.death};
                int[] counts={40,40,16,24},durations={100,50,40,80};
                for(int c=0;c<clips.Length;c++)
                {
                    var clip=clips[c];
                    Check(clip!=null&&clip.texture!=null&&clip.sheet.frames.Length==counts[c],key+" delivered clip loads: "+c);
                    Check(clip.texture.filterMode==FilterMode.Point&&clip.texture.mipmapCount==1,key+" native texture import: "+c);
                    Check(clip.sheet.anchorX==anchors[scenario]&&clip.sheet.anchorY==anchorY,key+" delivered anchor: "+c);
                    Check(clip.sheet.frames.All(f=>f.frame.w==size&&f.frame.h==size&&f.duration==durations[c]&&
                        f.frame.x>=0&&f.frame.y>=0&&f.frame.x+f.frame.w<=clip.texture.width&&f.frame.y+f.frame.h<=clip.texture.height),
                        key+" frames fit full-size texture with original timings: "+c);
                }
                string state=ViewSim.OrderedStateSnapshot;
                DrawBattle();
                Check(ViewSim.OrderedStateSnapshot==state,key+" drawing preserves simulation state");
                Check(ArtScale==1&&special.body.rectTransform.sizeDelta==new Vector2(size,size),key+" displayed at native 1x");
                Capture(key+"-start");
                var attackFrames=new HashSet<int>();bool capturedAttack=false;
                _paused=false;
                for(int frame=0;frame<240&&!ViewOver;frame++)
                {
                    yield return null;
                    if(special.drawnClip==special.attack)
                    {
                        attackFrames.Add(special.drawnFrame);
                        if(!capturedAttack&&special.drawnFrame>=28){Capture(key+"-attack");capturedAttack=true;}
                    }
                    if(frame%2==0)Capture(key+"-motion-"+(frame/2).ToString("D4"));
                }
                _paused=true;DrawBattle();
                Check(attackFrames.Count>1,key+" attack frames advance during the actual fight");
                File.WriteAllText(Path.Combine(_capture,key+"-frames.txt"),string.Join(",",attackFrames.OrderBy(f=>f)));
                Capture(key+"-paused");
                if(replay)
                    Check(CampaignSave.Write(_campaign)==archiveBefore,key+" replay leaves campaign unchanged");
                else
                {
                    if(special.unit.Alive)
                    {
                        Order(Directive.FocusFire);
                        Check(_targeting&&Click("target-"+special.unit.Id),key+" visible fighter can be targeted");
                        Check(ViewSim.FocusedId(0)==special.unit.Id,key+" command targets the authoritative fighter");
                    }
                    Save();state=_fight.Sim.OrderedStateSnapshot;
                    _campaign=CampaignSave.Read(File.ReadAllText(_savePath));ResumeFight();_pilotOn=false;
                    Check(_fight.Sim.OrderedStateSnapshot==state&&_figures.Any(f=>f.artKey==key&&f.body!=null),key+" save/reload retains the binding and state");
                }
            }
        }
    }
}
