using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KindredSiege.Sim.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Opt-in player test. Uses existing raycast/button validation with normal intro, coaching and scaling.
        // PointerEnter tests are synthetic engine events; they do not assert native mouse behaviour.
        IEnumerator PresentationTour()
        {
            var tour=PresentationTourRun();
            while(true)
            {
                object current=null;bool more=false;
                try{more=tour.MoveNext();if(more)current=tour.Current;}
                catch(Exception e)
                {
                    File.WriteAllText(Path.Combine(_manualReviewDir,"tour-FAILED.txt"),e+"\n"+_log);
                    Debug.LogException(e);Application.Quit(1);yield break;
                }
                if(!more)break;yield return current;
            }
            File.WriteAllText(Path.Combine(_manualReviewDir,"tour-PASS.txt"),_log.ToString());
            yield return new WaitForSecondsRealtime(.25f);
            Application.Quit(0);
        }
        void TourShot(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(_manualReviewDir,"tour-"+name+".png"));
        IEnumerator PresentationTourRun()
        {
            float until=Time.realtimeSinceStartup+12;
            while(_titleCard!=null&&Time.realtimeSinceStartup<until)yield return null;
            Check(_titleCard==null&&_page==Page.Title,"normal intro reaches title without automatic-mode bypass");
            TourShot("title");yield return new WaitForEndOfFrame();
            if(Argument("-ksArtTour")!="cards")
            {
            Check(Click("new"),"new campaign at actual window scale");yield return new WaitForSecondsRealtime(.15f);
            TourShot("tutorial-page");yield return new WaitForEndOfFrame();
            Check(Click("start-tutorial"),"tutorial start at actual window scale");yield return null;
            bool layout=Argument("-ksArtTour")=="layout";
            int[] orderButtons={0,2,3,1};
            for(int lesson=0;lesson<4;lesson++)
            {
                until=Time.realtimeSinceStartup+35;
                while(!(_coachStep==lesson&&_coachShown&&_paused)&&!_fight.Over&&Time.realtimeSinceStartup<until)yield return null;
                Check(_coachStep==lesson&&_coachShown&&_paused,"natural tutorial pause "+(lesson+1));
                TourShot("lesson-"+(lesson+1));yield return new WaitForEndOfFrame();
                if(layout)yield break;
                long tick=_fight.Sim.CurrentTick;yield return new WaitForSecondsRealtime(.25f);
                Check(_fight.Sim.CurrentTick==tick,"lesson "+(lesson+1)+" holds while waiting for the player");
                Check(Click("order-"+orderButtons[lesson]),"lesson "+(lesson+1)+" order button raycast");
                if(lesson==1)
                {
                    Figure target=null;foreach(var figure in _figures)if(figure.unit.Side==1&&figure.unit.Alive&&figure.unit.Row==Row.Front){target=figure;break;}
                    Check(target!=null,"a living enemy front target exists for Focus Fire");
                    Check(Click("target-"+target.unit.Id),"Focus Fire target raycast");
                }
                yield return null;
                Check(_coachStep==lesson+1&&!_paused,"lesson "+(lesson+1)+" resumes naturally");
            }
            Check(Click("speed")&&Click("speed"),"normal speed control advances for result coverage");
            until=Time.realtimeSinceStartup+65;
            while((!_fight.Over||_endAge<2.5f)&&Time.realtimeSinceStartup<until)yield return null;
            Check(_fight.Over&&Click("see-result"),"tutorial result reached after all four lessons");yield return null;
            TourShot("result");yield return new WaitForEndOfFrame();
            Check(Click("after-result"),"tutorial result continues");yield return new WaitForSecondsRealtime(.15f);
            }
            else
            {
                Check(Click("continue"),"reopen isolated review campaign for card coverage");
                yield return new WaitForSecondsRealtime(.15f);
            }
            Check(_campaign.Choosable().Count>0,"tribe choices available after tutorial");
            string tribeButton="tribe-"+_campaign.Choosable()[0].Id;
            bool clicked=Click(tribeButton);
            if(!clicked)
            {
                var b=_buttons[tribeButton];var r=b.GetComponent<RectTransform>();
                var p=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera,r.TransformPoint(r.rect.center))};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);
                Log("Failed card-entry raycast: interactable="+b.interactable+" pos="+p.position+" scale="+_canvas.scaleFactor+" hits="+string.Join(",",hits.ConvertAll(h=>h.gameObject.name)));
            }
            Check(clicked,"tribe selection reaches encounter cards");yield return new WaitForSecondsRealtime(.15f);
            TourShot("offers");yield return new WaitForEndOfFrame();
            int cards=0;
            foreach(var card in _screen.GetComponentsInChildren<RawImage>())
            {
                if(card.name!="Card")continue;
                var pointer=new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(card.gameObject,pointer,ExecuteEvents.pointerEnterHandler);
                yield return null;TourShot("card-"+cards+"-synthetic-hover");yield return new WaitForEndOfFrame();
                ExecuteEvents.Execute(card.gameObject,pointer,ExecuteEvents.pointerExitHandler);
                cards++;
            }
            Check(cards==_campaign.Offers.Count,"every card's synthetic hover rendered; native hover remains unverified");
            Log("Review used "+Screen.width+"x"+Screen.height+"; save path "+_savePath);
            yield return null;
        }
    }
}
