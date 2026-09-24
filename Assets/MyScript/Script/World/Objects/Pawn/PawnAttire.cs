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
    CancellationTokenSource wearCancellation;
    public async UniTask MoveToAndWear(Item item)
    {
        CancelWear();

        DataAttire attireData = item.itemData as DataAttire;
        if (attireData == null)
        {
            Debug.LogWarning($"Item {item.ThingName} is not an attire.");
            return;
        }

        wearCancellation = new CancellationTokenSource();

        reachDestination = false;
        destinationInvalid = false;
        item.reservingObject = this;
        await MakePathWithoutLast(item.CurrentGridPosition, PawnManager.Instance.GetWTS());
        
        try
        {
            wearCancellation.Token.ThrowIfCancellationRequested();

            while (!reachDestination)
            {
                if (item == null)
                {
                    CancelWear();
                }

                await UniTask.Yield(wearCancellation.Token);
            }

            if (destinationInvalid)
            {
                Debug.LogWarning($"Destination for item {item.ThingName} is invalid.");
                CancelWear();
                wearCancellation.Token.ThrowIfCancellationRequested();
            }

            if (!attireSprites.TryGetValue(attireData.bodyTag, out SpriteAttire attireSprite) || !attireSprite)
            {
                Debug.LogWarning($"No attire sprite found for body tag {attireData.bodyTag}.");
            }
            else
            {
                await CreateWearProgress(item, attireSprite.attireData.wearingTime, wearCancellation.Token);
                wearCancellation.Token.ThrowIfCancellationRequested();

                attireSprites.Remove(attireData.bodyTag);

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
            }


            await CreateWearProgress(item, attireData.wearingTime, wearCancellation.Token);
            wearCancellation.Token.ThrowIfCancellationRequested();

            CreateAttireSprite(
                attireData,
                item.itemClass,
                attireData.bodyTag,
                false
            );
        }
        finally
        {
            if (item)
            {
                item.reservingObject = null;
            }
            wearCancellation?.Dispose();
            wearCancellation = null;
        }
    }
    async UniTask CreateWearProgress(Item item, float wearingTime, CancellationToken token)
    {
        try
        {
            CreateProgressBar();
            float elapsedTime = 0f;

            while (elapsedTime < wearingTime)
            {
                if (item == null)
                {
                    CancelWear();
                }

                await UniTask.Yield(token);

                elapsedTime += Time.deltaTime * WORK_PROGRESS_PER_SECOND;
                progressBarInstance.SetProgress(Mathf.Clamp01(elapsedTime / wearingTime));
            }
        }
        finally
        {
            DestroyProgressBar();
        }
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
            wearCancellation?.Dispose();
            wearCancellation = null;
        }
    }
    async UniTask StartWear(
        DataAttire attireData,
        ItemClass itemClass,
        bool debug)
    {
        CancelWear();

        var cts = new CancellationTokenSource();
        wearCancellation = cts;

        try
        {
            await WearOperation(attireData, itemClass, debug, cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (wearCancellation == cts)
                wearCancellation = null;

            cts.Dispose();
        }
    }
    async UniTask StartUndress(BodyTag bodyTag)
    {
        CancelWear();

        var cts = new CancellationTokenSource();
        wearCancellation = cts;

        try
        {
            await Undress(bodyTag, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Wear cancelled.
        }
        finally
        {
            if (wearCancellation == cts)
                wearCancellation = null;

            cts.Dispose();
        }
    }
    async UniTask<bool> WearOperation(
    DataAttire attireData,
    ItemClass itemClass,
    bool debug,
    CancellationToken cancellationToken)
    {

        if (!attireData.HasAttirePart(genome.currentBody.bodyPartShape))
        {
            Debug.LogWarning(
                $"Failed to wear {attireData.thingName} due to body part shape mismatch"
            );

            return false;
        }

        // Cancel can also happen while undressing.
        await Undress(attireData.bodyTag, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        await RunWearProgressAsync(attireData.wearingTime, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        CreateAttireSprite(
            attireData,
            itemClass,
            attireData.bodyTag,
            debug
        );

        return true;
    }

    async UniTask<bool> Undress(
    BodyTag bodyTag,
    CancellationToken cancellationToken = default)
    {
        if (!attireSprites.TryGetValue(bodyTag, out SpriteAttire attireSprite) || !attireSprite)
        {
            return false;
        }

        await RunWearProgressAsync(
            attireSprite.attireData.wearingTime,
            cancellationToken
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
        CancellationToken cancellationToken)
    {
        ProgressBar progressBar = CreateProgressBar();
        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < duration)
            {
                await UniTask.Yield(cancellationToken);

                elapsedTime += Time.deltaTime * WORK_PROGRESS_PER_SECOND;
                progressBar.SetProgress(Mathf.Clamp01(elapsedTime / duration));
            }

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
