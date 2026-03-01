using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetManager
{
    #region Singleton
    private static AssetManager _instance;
    public static AssetManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AssetManager();
            return _instance;
        }
    }

    #endregion
    public struct AssetArguments<T>
    {
        /// <summary> 다운로드 그룹에 속한 어드레스 주소 </summary>
        public string address;
        /// <summary> 성공 시, 액션 </summary>
        public Action<T> successCallback;
        /// <summary> 실패 시, 액션 </summary>
        public Action failedCallback;
    }

    private Dictionary<string, object> addressablePacket = new Dictionary<string, object>();

    internal void LoadAssetAsync<T>(AssetArguments<T> args)
    {
        if (string.IsNullOrEmpty(args.address) == true)
        {
            Debug.LogError("AddressableDownloadAgrs is Null");
            return;
        }

        if (addressablePacket.TryGetValue(args.address, out object reObj))
        {
            args.successCallback?.Invoke((T)reObj);
            return;
        }

        Addressables.LoadAssetAsync<T>(args.address).Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                // 캐시 선 등록
                if (!addressablePacket.ContainsKey(args.address))
                    addressablePacket.Add(args.address, op.Result);

                args.successCallback?.Invoke(op.Result);
            }
            else
                Debug.LogError($"Not Load Asset {typeof(T)} Address : {args.address}");
        };
    }

    internal void LoadGameObjectAsync(AssetArguments<GameObject> args, Transform parent = null)
    {
        if (string.IsNullOrEmpty(args.address))
        {
            Debug.LogError("AddressableDownloadAgrs is Null");
            args.failedCallback?.Invoke();
            return;
        }

        Action<GameObject> originalSuccessCallback = args.successCallback;
        args.successCallback = (loadedPrefab) =>
        {
            if (loadedPrefab != null)
            {
                // 프리팹을 부모 Transform 아래에 실제 게임 오브젝트로 생성합니다.
                GameObject instance = UnityEngine.Object.Instantiate(loadedPrefab, parent);
                originalSuccessCallback?.Invoke(instance);
            }
            else
            {
                Debug.LogError($"Failed to instantiate Asset: {args.address}");
                args.failedCallback?.Invoke();
            }
        };

        LoadAssetAsync(args);

    }

    internal GameObject LoadGameObject(string address, Transform parent = null)
    {
        GameObject gameObject = LoadAsset<GameObject>(address);
        if (gameObject != null)
        {
            if (addressablePacket.ContainsKey(address) == false)
                addressablePacket.Add(address, gameObject);
            
            return UnityEngine.Object.Instantiate(gameObject, parent);
        }
        else
        {
            Debug.LogError($"Failed to load Asset: {address}");
            return null;
        }
    }

    internal T LoadAsset<T>(string address)
    {
        if (addressablePacket.TryGetValue(address, out object reObj))
            return (T)reObj;
        else
        {
            object newObj = Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
            if (addressablePacket.ContainsKey(address) == false)
                addressablePacket.Add(address, newObj);
            return (T)newObj;
        }

    }

}
