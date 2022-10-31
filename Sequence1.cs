
using AK.Wwise;

public class Sequence1 : GameState
{
    public override void EnterState(GameManager game)
    {
        //Initiate music, rail etc !
        game.ara.SetActive(true);
        game.audioManager.activateOnSeq1.ForEach(e => e.Post(game.mainCamera.gameObject));
        game.MuteInput(false);
        game.tutorialGameObject.SetActive(false);
        game.kaiCart.m_Speed = 35;
        game.cameraManager.isTutorial = false;
        game.araManager.Unscript();
        game.BlackBands.SwitchBlackBands(false);
    }

    public override void UpdateState(GameManager game)
    {
        //Gameplay logic we need to have
    }

    public override void LeaveState(GameManager game)
    {
        game.audioManager.disactivateOnSeq1.ForEach(e => e.Stop(game.mainCamera.gameObject, game.transitionAudio, AkCurveInterpolation.AkCurveInterpolation_Sine));
    }
}
