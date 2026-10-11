using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class Pawn
{
    [Header("Attire")]
    [SerializeField] SpriteAttire attirePrefab;
    private readonly Dictionary<BodyTag, SpriteAttire> attireSprites = new();
    private CancellationTokenSource wearCancellation;
    private readonly SemaphoreSlim attireOperationGate = new(1, 1);

    public async UniTask<bool> MoveToAndWear(Item item)
    {
        return await RunAttireOperation(async token =>
        {
            DataAttire attireData = item.itemData as DataAttire;
            if (attireData == null)
            {
                Debug.LogWarning($"Item {item.ThingName} is not an attire.");
                return false;
            }

            if (!CanWear(attireData))
            {
                return false;
            }

            reachDestination = false;
            destinationInvalid = false;
            item.reservingObject = this;

            try
            {
                await MakePathWithoutLast(item.CurrentGridPosition, PawnManager.Instance.GetWTS());
                token.ThrowIfCancellationRequested();

                while (!reachDestination && !destinationInvalid)
                {
                    ValidateWearItem(item, token);
                    await UniTask.Yield(token);
                }

                ValidateWearItem(item, token);
                if (destinationInvalid)
                {
                    Debug.LogWarning($"Destination for item {item.ThingName} is invalid.");
                    return false;
                }

                return await WearOperation(
                    attireData,
                    item.itemClass,
                    false,
                    token,
                    () => ValidateWearItem(item, token));
            }
            finally
            {
                if (item)
                {
                    item.reservingObject = null;
                }
            }
        });
    }

    private void ValidateWearItem(Item item, CancellationToken token)
    {
        if (item == null)
        {
            CancelWear();
        }

        token.ThrowIfCancellationRequested();
    }

    public void Wear(DataAttire attireData, ItemClass itemClass, bool debug = false)
    {
        StartWear(attireData, itemClass, debug).Forget();
    }

    public void Undress(BodyTag bodyTag)
    {
        StartUndress(bodyTag).Forget();
    }

    public void CancelWear(bool dispose = false)
    {
        wearCancellation?.Cancel();
        if (dispose)
        {
            // The running operation disposes its source after its awaits finish.
            wearCancellation = null;
        }
    }

    private async UniTask StartWear(DataAttire attireData, ItemClass itemClass, bool debug)
    {
        await RunAttireOperation(token =>
            WearOperation(attireData, itemClass, debug, token, () => { }));
    }

    private async UniTask StartUndress(BodyTag bodyTag)
    {
        await RunAttireOperation(token => Undress(bodyTag, token));
    }

    private async UniTask<bool> RunAttireOperation(Func<CancellationToken, UniTask<bool>> operation)
    {
        CancelWear();

        var cancellationSource = new CancellationTokenSource();
        wearCancellation = cancellationSource;
        CancellationToken token = cancellationSource.Token;
        bool enteredGate = false;

        try
        {
            await attireOperationGate.WaitAsync(token);
            enteredGate = true;
            token.ThrowIfCancellationRequested();
            return await operation(token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            if (enteredGate)
            {
                attireOperationGate.Release();
            }

            if (wearCancellation == cancellationSource)
            {
                wearCancellation = null;
            }

            cancellationSource.Dispose();
        }
    }

    private bool CanWear(DataAttire attireData)
    {
        if (!attireData.HasAttirePart(genome.currentBody.bodyPartShape))
        {
            Debug.LogWarning(
                $"Failed to wear {attireData.thingName} due to body part shape mismatch");
            return false;
        }

        return true;
    }
    async UniTask<bool> WearOperation(
        DataAttire attireData,
        ItemClass itemClass,
        bool debug,
        CancellationToken cancellationToken,
        Action validateItem)
    {
        if (!CanWear(attireData))
        {
            return false;
        }

        // Cancel can also happen while undressing.
        await Undress(attireData.bodyTag, cancellationToken, validateItem);

        cancellationToken.ThrowIfCancellationRequested();

        await RunWearProgressAsync(attireData.wearingTime, cancellationToken, validateItem);

        cancellationToken.ThrowIfCancellationRequested();

        CreateAttireSprite(
            attireData,
            itemClass,
            attireData.bodyTag,
            debug
        );

        return true;
    }

    private UniTask<bool> Undress(BodyTag bodyTag, CancellationToken cancellationToken)
    {
        return Undress(bodyTag, cancellationToken, () => { });
    }

    private async UniTask<bool> Undress(
        BodyTag bodyTag,
        CancellationToken cancellationToken,
        Action validateItem)
    {
        if (!attireSprites.TryGetValue(bodyTag, out SpriteAttire attireSprite) || !attireSprite)
        {
            return false;
        }

        await RunWearProgressAsync(
            attireSprite.attireData.wearingTime,
            cancellationToken,
            validateItem
        );

        cancellationToken.ThrowIfCancellationRequested();

        attireSprites.Remove(bodyTag);

        if (!attireSprite.debug)
        {
            world.CreateItem(
                currentGridPos,
                attireSprite.attireData,
                attireSprite.itemClass,
                1,
                null
            );
        }

        Destroy(attireSprite.gameObject);

        return true;
    }

    private async UniTask RunWearProgressAsync(
        float duration,
        CancellationToken cancellationToken,
        Action validateItem)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ProgressBar progressBar = CreateProgressBar();
        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < duration)
            {
                validateItem();
                await UniTask.Yield(cancellationToken);
                validateItem();

                elapsedTime += Time.deltaTime * WORK_PROGRESS_PER_SECOND;
                progressBar.SetProgress(Mathf.Clamp01(elapsedTime / duration));
            }

            validateItem();
            cancellationToken.ThrowIfCancellationRequested();
        }
        finally
        {
            DestroyProgressBar(progressBar);
        }
    }

    public SpriteAttire GetAttireSprite(BodyTag bodyTag)
    {
        attireSprites.TryGetValue(bodyTag, out SpriteAttire attireSprite);
        return attireSprite;
    }

    void ChangeAttireDirection(Direction dir)
    {
        foreach (SpriteAttire attire in attireSprites.Values)
        {
            if (attire)
            {
                attire.SetDirection(dir);
            }
        }
    }

    void UpdateAttireLayer()
    {
        foreach (SpriteAttire attire in attireSprites.Values)
        {
            if (attire)
            {
                attire.UpdateLayer();
            }
        }
    }

    void CreateAttireSprite(DataAttire attire, ItemClass itemClass, BodyTag bodyTag, bool debug)
    {
        SpriteAttire attireSprite = null;
        Transform parent = bodyTag switch
        {
            BodyTag.OffBody => transform,
            BodyTag.Head => headData?.spriteTransform,
            BodyTag.Torso => bodyData.spriteTransform,
            BodyTag.Legs => bodyData.spriteTransform,
            _ => null
        };
        PartBioSprite parentPart = bodyTag switch
        {
            BodyTag.Head => headData?.SpritePart,
            BodyTag.Torso => bodyData.SpritePart,
            BodyTag.Legs => bodyData.SpritePart,
            _ => null
        };
        if (parent != null)
        {
            attireSprite = Instantiate(attirePrefab, parent);
            attireSprite.Initialize(this, attire, itemClass, parentPart, debug);
        }
        attireSprites[bodyTag] = attireSprite;
    }
}
