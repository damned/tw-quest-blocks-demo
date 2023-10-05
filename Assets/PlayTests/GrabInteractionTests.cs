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
    private XRDirectInteractor directInteractor;
    private XRController controller;
    private Mesh sphereMesh;
    private Action<XRControllerRecording> frameAdder;


    [OneTimeSetUp]
    public void Init()
    {
        var templateSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        templateSphere.transform.position = new Vector3(100, 100, 100);
        sphereMesh = templateSphere.GetComponent<MeshFilter>().mesh;
    }

    [SetUp]
    public void TearDown()
    {
        TestUtilities.DestroyAllSceneObjects();
    }

    [SetUp]
    public void SetUp()
    {
        Physics.autoSimulation = true;
        new GameObject().AddComponent<FrameWatcher>();

        TestUtilities.CreateInteractionManager();
        directInteractor = TestUtilities.CreateDirectInteractor();
        controller = directInteractor.GetComponent<XRController>();
        MakeSphereVisible(controller.gameObject);
    }

    [UnityTest]
    public IEnumerator ShouldMoveAGrabbedKinematicBlockWithHand()
    {
        var magnetObject = BlockBuilder.CreateMagnetAndParentBlock();

        var block = magnetObject.transform.parent.gameObject;
        block.GetComponent<Rigidbody>().isKinematic = true;

        var interactable = block.AddComponent<XRGrabInteractable>();
        interactable.useDynamicAttach = true;

        FaceOriginFrom(block, new Vector3(0, 0, 1f));

        frameAdder = (recording) =>
        {
            Frame().AtTime(0f).WithSelect(false).AddTo(recording);
            Frame().AtTime(0.1f).WithSelect(false).AtPosition(0f, 0.5f, 0.2f).AddTo(recording);
            Frame().AtTime(0.2f).WithSelect(true).AtPosition(0f, 0.5f, 0.5f).AddTo(recording);
            Frame().AtTime(0.3f).WithSelect(true).AtPosition(0f, 0.5f, 1f).AddTo(recording);
        };

        PlayFrames(frameAdder);
        yield return WaitForFrameAfterElapsedTime(0.3f);

        Assert.That(directInteractor.interactablesSelected, Is.EqualTo(new[] { interactable }));
        Assert.AreEqual(1.5f, block.transform.position.z, 0.01f);
    }

    [UnityTest]
    public IEnumerator ShouldGrabAnInteractable()
    {
        var interactable = TestUtilities.CreateGrabInteractable();
        frameAdder = (recording) =>
        {
            Frame().AtTime(0f).WithSelect(false).AddTo(recording);
            Frame().AtTime(0.1f).WithSelect(true).AddTo(recording);
        };
        var controllerRecorder = PlayFrames(frameAdder);
        yield return WaitForFrameAfterElapsedTime(0.05f);
        Debug.Log("current time after wait for seconds: " + controllerRecorder.currentTime);

        Assert.That(directInteractor.interactablesSelected, Is.EqualTo(new[] { interactable }));
    }

    private XRControllerRecorder PlayFrames(Action<XRControllerRecording> addRecordingFrames)
    {
        var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, addRecordingFrames);
        controllerRecorder.isPlaying = true;
        return controllerRecorder;
    }

    private void Move(Component component, Vector3 position)
    {
        component.gameObject.transform.position = position;
    }

    private void FaceOriginFrom(GameObject go, Vector3 position)
    {
        go.transform.position = position;
        go.transform.LookAt(Vector3.zero);
    }

    private static WaitForSeconds WaitForFrameAfterElapsedTime(float elapsedTime)
    {
        return new WaitForSeconds(elapsedTime);
    }

    public void MakeSphereVisible(GameObject sphere, float scale = 0.1f)
    {
        sphere.GetComponent<SphereCollider>().radius = 0.5f;
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.AddComponent<MeshFilter>().mesh = sphereMesh;
        sphere.AddComponent<MeshRenderer>();
    }

    private ControllerStateBuilder Frame()
    {
        return new ControllerStateBuilder();
    }

    public class ControllerStateBuilder
    {
        private float frameTime = 0f;
        private bool selectActive = false;
        private Vector3 position = Vector3.zero;

        public ControllerStateBuilder AtTime(float t)
        {
            frameTime = t;
            return this;
        }

        public ControllerStateBuilder WithSelect(bool isActive)
        {
            selectActive = isActive;
            return this;
        }

        public ControllerStateBuilder AtPosition(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
            return this;
        }

        public XRControllerState Build()
        {
            return new XRControllerState(frameTime, position, Quaternion.identity, InputTrackingState.All, true, selectActive, false, false);
        }

        public void AddTo(XRControllerRecording recording)
        {
            recording.AddRecordingFrameNonAlloc(Build());
        }
    }

    public class FrameWatcher : MonoBehaviour
    {
        private int frameNumber = 0;

        void Update()
        {
            Debug.Log("got an update, frame: " + frameNumber++);
        }
    }

}
