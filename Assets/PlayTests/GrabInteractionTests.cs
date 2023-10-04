using System;
using System.Collections;

using NUnit.Framework;

using UnityEngine;

using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.TestSupport;


[TestFixture]    
public class GrabInteractionTests
{
    Vector3EqualityComparer closeEnoughVectorComparer = Vector3EqualityComparer.Instance;
    
    [SetUp]
    public void TearDown()
    {
        TestUtilities.DestroyAllSceneObjects();
    }

    public class FrameWatcher : MonoBehaviour
    {
        private int frameNumber = 0;

        void Update()
        {
            Debug.Log("got an update, frame: " + frameNumber++);
        }
    }


    [SetUp]
    public void SetUp()
    {
        Physics.autoSimulation = true;
    }

    [UnityTest]
    public IEnumerator ShouldGrabAnInteractable() // TODO: ShouldMoveAGrabbedBlockWithHand
    {
        var frameWatcher = new GameObject().AddComponent<FrameWatcher>();

        TestUtilities.CreateInteractionManager();
        var interactable = TestUtilities.CreateGrabInteractable();
        var directInteractor = TestUtilities.CreateDirectInteractor();
        var controller = directInteractor.GetComponent<XRController>();
        var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
        {
            recording.AddRecordingFrameNonAlloc(new XRControllerState(0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                false, false, false));
            recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                true, false, false));
        });
        controllerRecorder.isPlaying = true;
        yield return WaitForFrameAfterElapsedTime(0.05f);
        Debug.Log("current time after wait for seconds: " + controllerRecorder.currentTime);

        Assert.That(directInteractor.interactablesSelected, Is.EqualTo(new[] { interactable }));
    }

    private static WaitForSeconds WaitForFrameAfterElapsedTime(float elapsedTime)
    {
        return new WaitForSeconds(elapsedTime);
    }

    public class ControllerStateBuilder
    {
        public XRControllerState Build()
        {
            float frameTime = 0f;
            bool selectActive = false;

            return new XRControllerState(frameTime, Vector3.zero, Quaternion.identity, InputTrackingState.All, true, selectActive, false, false);
        }

        public void AddTo(XRControllerRecording recording)
        {
            recording.AddRecordingFrameNonAlloc(Build());
        }
    }

}
