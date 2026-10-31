using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Opt-in observation only. No automatic commands, scene changes or simulation changes.
        string _manualReviewDir;
        [Serializable] sealed class ManualReviewState
        {
            public string page, savePath, hover, notice, targeting, selectedControl;
            public int width, height, coachStep;
            public long tick;
            public bool paused, coachShown, titleCard, modal;
        }

        IEnumerator ManualReview()
        {
            float started=Time.realtimeSinceStartup, nextCapture=0;
            int shot=0;string previous=null;
            while(true)
            {
                yield return new WaitForEndOfFrame();
                float elapsed=Time.realtimeSinceStartup-started;
                var state=new ManualReviewState
                {
                    page=_page.ToString(),savePath=_savePath,hover=_hover,notice=_notice,
                    targeting=_targeting?"pick target":"off",width=Screen.width,height=Screen.height,
                    selectedControl=UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==null?"":UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name,
                    coachStep=_coachStep,paused=_paused,coachShown=_coachShown,titleCard=_titleCard!=null,
                    modal=_modal!=null,tick=_fight==null?-1:_fight.Sim.CurrentTick
                };
                string json=JsonUtility.ToJson(state,true);
                bool changed=previous==null||state.page!=_manualLastPage||state.coachStep!=_manualLastCoach||state.paused!=_manualLastPause;
                if((Argument("-ksArtTour")==null&&((elapsed<6f&&elapsed>=nextCapture)||changed))||Input.GetKeyDown(KeyCode.F12))
                {
                    string name=(shot++).ToString("D4")+"-"+_page;
                    ScreenCapture.CaptureScreenshot(Path.Combine(_manualReviewDir,name+".png"));
                    File.WriteAllText(Path.Combine(_manualReviewDir,name+".json"),json);
                    nextCapture=elapsed+.2f;
                }
                if(json!=previous){File.WriteAllText(Path.Combine(_manualReviewDir,"current-state.json"),json);previous=json;}
                _manualLastPage=state.page;_manualLastCoach=state.coachStep;_manualLastPause=state.paused;
            }
        }
        string _manualLastPage;
        int _manualLastCoach;
        bool _manualLastPause;
    }
}
