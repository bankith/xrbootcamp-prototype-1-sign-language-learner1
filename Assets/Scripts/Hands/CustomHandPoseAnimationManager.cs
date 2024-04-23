using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHandPoseAnimationManager : MonoBehaviour
{
public float poseTransitionDuration = 3f;
    
    public GameObject handStartPrefab;
    
    public CustomHandData startHandPose;
    public CustomHandData nextHandPose;

    private Vector3 startingHandPosition;
    private Vector3 finalHandPosition;
    private Quaternion startingHandRotation;
    private Quaternion finalHandRotation;

    private Quaternion[] startingFingerRotations;
    private Quaternion[] finalFingerRotations;

    private GameObject _handStartPrefab;
    private bool IsStopAnimation;
    private bool IsRunning;
    
     
    [SerializeField]
    private Renderer _hand1Renderer;
    [SerializeField]
    private Renderer _hand2Renderer;
    
    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        _handStartPrefab = Instantiate(handStartPrefab, startHandPose.transform.position, handStartPrefab.transform.localRotation);
        _handStartPrefab.SetActive(false);
        // ActivateTransition();
        _hand2Renderer.enabled = false;
    }

    public void ActivateTransition()
    {
        if(IsRunning) {return;}
        IsRunning = true;
        
        _hand1Renderer.enabled = false;
        _handStartPrefab.SetActive(true);
        SetHandDataValues(startHandPose, nextHandPose);
        coroutine = SetHandDataRoutine(_handStartPrefab.GetComponent<CustomHandData>(), finalHandPosition,
            finalHandRotation, finalFingerRotations, startingHandPosition, startingHandRotation,
            startingFingerRotations);
        StartCoroutine(coroutine);
    }

    private void OnDisable()
    {
        Destroy(_handStartPrefab);
        IsStopAnimation = true;
    }


    public void SetHandDataValues(CustomHandData h1, CustomHandData h2)
    {
        startingHandPosition = new Vector3( h1.root.position.x / h1.root.localScale.x,
            h1.root.position.y / h1.root.localScale.y, h1.root.position.z / h1.root.localScale.z);
        finalHandPosition = new Vector3(h2.root.position.x / h2.root.localScale.x,
            h2.root.position.y / h2.root.localScale.y, h2.root.position.z / h2.root.localScale.z);

        startingHandRotation = h1.root.localRotation;
        finalHandRotation = h2.root.localRotation;

        startingFingerRotations = new Quaternion[h1.fingerBones.Length];
        finalFingerRotations = new Quaternion[h1.fingerBones.Length];

        for (int i = 0; i < h1.fingerBones.Length; i++)
        {
            startingFingerRotations[i] = h1.fingerBones[i].localRotation;
            finalFingerRotations[i] = h2.fingerBones[i].localRotation;
        }
    }

    public void SetHandData(CustomHandData h, Vector3 newPosition, Quaternion newRotation, Quaternion[] newBonesRotation)
    {
        h.root.position = newPosition;
        h.root.localRotation = newRotation;

        for (int i = 0; i < newBonesRotation.Length; i++)
        {
            h.fingerBones[i].localRotation = newBonesRotation[i];
        }
    }

    public IEnumerator SetHandDataRoutine(CustomHandData h, Vector3 newPosition, Quaternion newRotation, Quaternion[] newBonesRotation,Vector3 startingPosition, Quaternion startingRotation, Quaternion[] startingBonesRotation)
    {
        float timer = 0;

        while(timer < poseTransitionDuration)
        {
            Vector3 p = Vector3.Lerp(startingPosition, newPosition, timer / poseTransitionDuration);
            Quaternion r = Quaternion.Lerp(startingRotation, newRotation, timer / poseTransitionDuration);

            h.root.position = p;
            h.root.localRotation = r;

            for (int i = 0; i < newBonesRotation.Length; i++)
            {
                h.fingerBones[i].localRotation = Quaternion.Lerp(startingBonesRotation[i], newBonesRotation[i], timer / poseTransitionDuration);
            }

            timer += Time.deltaTime;
            yield return null;
        }
        
        h.root.position = newPosition;
        h.root.localRotation = newRotation;

        for (int i = 0; i < newBonesRotation.Length; i++)
        {
            h.fingerBones[i].localRotation = newBonesRotation[i];
        }
        for (int i = 0; i < newBonesRotation.Length; i++)
        {
            h.fingerBones[i].localRotation = newBonesRotation[i];
        }

        _hand2Renderer.enabled = true;
        
        
        if (!IsStopAnimation)
        {
            yield return new WaitForSeconds(1);
            ActivateTransition();
        }
    }


    public void MirrorPose(CustomHandData poseToMirror, CustomHandData poseUsedToMirror)
    {
        Vector3 mirroredPosition = poseUsedToMirror.root.position;
        mirroredPosition.x *= -1;

        Quaternion mirroredQuaternion = poseUsedToMirror.root.localRotation;
        mirroredQuaternion.y *= -1;
        mirroredQuaternion.z *= -1;

        poseToMirror.root.position = mirroredPosition;
        poseToMirror.root.localRotation = mirroredQuaternion;

        for (int i = 0; i < poseUsedToMirror.fingerBones.Length; i++)
        {
            poseToMirror.fingerBones[i].localRotation = poseUsedToMirror.fingerBones[i].localRotation;
        }
    }

    public void ResetHands()
    {
        DisableAll();
        _hand1Renderer.enabled = true;
    }

    public void DisableAll()
    {
        IsStopAnimation = true;
        IsRunning = false;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        _hand1Renderer.enabled = false;
        _hand2Renderer.enabled = false;
        _handStartPrefab.SetActive(false);
    }
    
    public void OnFinished()
    {
        DisableAll();
    }
}
