using System;
using UnityEngine;
using Tools;

public enum GameState
{
    Boot,
    Playing,
    Paused,
    LevelComplete
}

[DisallowMultipleComponent]
public class GameFlowManager : Singleton<GameFlowManager>
{
    [Header("关卡与状态")]
    public GameState state = GameState.Boot;

    [Header("色卡收集")]
    public int swatchesCollected = 0;
    public int swatchesNeeded = 3; // 策划：集齐3张色卡，通关后解锁彩蛋

    // === 这些事件给将来 UI 或玩法脚本用（可选）===
    public event Action<GameState> OnGameStateChanged;
    public event Action<int, int> OnSwatchCountChanged; // (当前, 需要)

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 启动即进入游戏
        SetState(GameState.Playing);
    }

    public void SetState(GameState newState)
    {
        if (state == newState) return;
        state = newState;
        OnGameStateChanged?.Invoke(state);
    }

    // ============ 供玩法脚本调用的流程接口 ============
    public void OnSwatchCollected()
    {
        swatchesCollected = Mathf.Clamp(swatchesCollected + 1, 0, swatchesNeeded);
        OnSwatchCountChanged?.Invoke(swatchesCollected, swatchesNeeded);
        AudioManager.Instance.PlaySFX("CardPickup");

        if (swatchesCollected >= swatchesNeeded)
        {
            // 比如：点亮三色大门 -> 开门
            OnDoorCanOpen();
        }
    }

    public void OnChestOpened()
    {
        // 可加统计/提示
        AudioManager.Instance.PlaySFX("ChestOpen");
    }

    public void OnDoorCanOpen()
    {
        // 大门可开
        AudioManager.Instance.PlaySFX("DoorOpen");
        // 如果你做了 Door 脚本，这里可发消息或直接查找 Door 打开
        // FindObjectOfType<Door>()?.Open();
    }

    public void OnLevelCompleted()
    {
        SetState(GameState.LevelComplete);
        // 可在这里切场景或展示结算
    }

    public void PauseGame(bool pause)
    {
        SetState(pause ? GameState.Paused : GameState.Playing);
        Time.timeScale = pause ? 0f : 1f;
    }

    // ============ UI 回调（你后面做 UI 时直接绑） ============
    public void OnClickShoot() { AudioManager.Instance.PlaySFX("Shoot"); /* TODO: 调用玩家发射 */ }
    public void OnClickSelectInkA1() { /* 选红 */  AudioManager.Instance.PlaySFX("PickColor"); }
    public void OnClickSelectInkA2() { /* 选黄 */  AudioManager.Instance.PlaySFX("PickColor"); }
    public void OnClickSelectInkA3() { /* 选蓝 */  AudioManager.Instance.PlaySFX("PickColor"); }
    public void OnClickSelectInkB() { /* 选混色槽 */ }
    public void OnClickMixA1TabA2() { /* 红+黄=橙 ... 具体混色逻辑放玩家/背包系统 */ }
    public void OnClickMixA1TabA3() { /* 红+蓝=紫 */ }
    public void OnClickMixA2TabA3() { /* 黄+蓝=绿 */ }

    // 供触发器调用（比如玩家捡到色卡的触发器上勾选）
    public void CollectSwatch_ByTrigger() => OnSwatchCollected();
}
