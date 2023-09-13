using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    #region Config
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;

    [SerializeField] private Transform handTransform;
    [SerializeField] private MeshRenderer handObject;

    [SerializeField] Camera camera;
    #endregion


    //We can create scriptable objects that can map buttons to actions
    public enum ObjectColors
    {
        White,
        Red,
        Green,
        Blue,
    }

    public static Dictionary<ObjectColors, Color> ColorMap = new Dictionary<ObjectColors, Color>()
    {
        {ObjectColors.White, Color.white },
        {ObjectColors.Red, Color.red},
        {ObjectColors.Green, Color.green},
        {ObjectColors.Blue, Color.blue},
    };



    void Start()
    {
        if (!photonView.IsMine)
        {
            DisableVRComponents();
        }
        InitializeHand();
    }

    void DisableVRComponents()
    {
        camera.enabled = photonView.IsMine;
        foreach (var driver in GetComponentsInChildren<TrackedPoseDriver>())
            Destroy(driver);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        Move();
        Rotate();
        CheckChangeColor();
    }

    void Move()
    {
        var verticalValue = Input.GetAxis("XRI_Left_Primary2DAxis_Vertical");
        var horizontalValue = Input.GetAxis("XRI_Left_Primary2DAxis_Horizontal");

        transform.position -= moveSpeed * verticalValue * Time.deltaTime * transform.forward;
        transform.position += moveSpeed * horizontalValue * Time.deltaTime * transform.right;

    }

    void Rotate()
    {
        var rotate = Input.GetAxis("XRI_Right_Primary2DAxis_Horizontal");
        transform.Rotate(new Vector3(0, rotateSpeed * rotate * Time.deltaTime, 0));
    }

    void CheckChangeColor()
    {
        if (!LevelManager.Instance.ColorInteractionsEnabled) return;

        if (Input.GetButton("XRI_Left_PrimaryButton"))
        {
            ChangeColor(ObjectColors.Red);
        }
        else if (Input.GetButton("XRI_Left_SecondaryButton"))
        {
            ChangeColor(ObjectColors.Blue);
        }
        else if (Input.GetButton("XRI_Right_PrimaryButton"))
        {
            ChangeColor(ObjectColors.White);
        }
        else if (Input.GetButton("XRI_Right_SecondaryButton"))
        {
            ChangeColor(ObjectColors.Green);
        }
    }

    void ClearHand()
    {
        if(handObject) Destroy(handObject.gameObject);
    }

    void InitializeHand()
    {
        if (!photonView.IsMine) return;

        ClearHand();
        ObjectColors color = GetColor();
        handObject = Instantiate(LevelManager.Instance.HandObjectPrefab, handTransform).GetComponent<MeshRenderer>();

        photonView.RPC(nameof(RPC_ChangeColor), RpcTarget.AllBuffered, color);
    }

    void ChangeColor(ObjectColors color)
    {
        if (!photonView.IsMine) return;


        Hashtable props = new Hashtable();
        string sceneColorProp = $"{GameManager.Instance.LocalScene.name}Color";
        props[sceneColorProp] = color;
        photonView.Owner.SetCustomProperties(props);
        
        photonView.RPC(nameof(RPC_ChangeColor), RpcTarget.AllBuffered, color);
    }

    ObjectColors GetColor()
    {
        string sceneColorProp = $"{GameManager.Instance.LocalScene.name}Color";
        if (photonView.Owner.CustomProperties.TryGetValue(sceneColorProp, out object colorObject))
        {
            return (ObjectColors)colorObject;
        }
        else return ObjectColors.White;
    }


    [PunRPC]
    void RPC_ChangeColor(ObjectColors objectColor)
    {
        Debug.Log($"[Player][RPC] Change Color {objectColor}");
        if(!handObject)
            handObject = Instantiate(LevelManager.Instance.HandObjectPrefab, handTransform).GetComponent<MeshRenderer>();
        Color color = ColorMap[objectColor];
        handObject.material.color = color;
    }
}
