/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */
using Jyx2;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

//使用物品状态
public class UseItemState : IBattleState
{
    RoleInstance currentRole;
    Jyx2Item currentItem;
    float castAnimLenght = 0.5f;//使用物品动画时间1s
    public override void OnEnterState()
    {
        currentRole = BattleStateMechine.Instance.CurrentRole;
        currentItem = BattleStateMechine.Instance.CurrentUseItem;
        if (currentRole == null || currentItem == null) 
        {
            GameUtil.LogError("使用物品状态错误");
            return;
        }
        UseItem();
    }

    void UseItem() 
    {
        /*
        if (currentRole.View)
        {
            var animator = currentRole.View.GetAnimator();
            animator.SetTrigger("cast");
        }
        Observable.TimerFrame(Convert.ToInt32(castAnimLenght * 60), FrameCountType.FixedUpdate)
            .Subscribe(ms =>
            {
                OnAnimOver();
            });
            */

        if (!currentRole.View)
        {
            OnAnimOver();
            return;
        }
        
        AnimationClip clip = null;
        var itemType = currentItem.GetItemType();
        if (itemType == Jyx2ItemType.Costa)
            clip = GlobalAssetConfig.Instance.useItemClip; //选择吃药的动作
        else if (itemType == Jyx2ItemType.Anqi)
            clip = GlobalAssetConfig.Instance.anqiClip; //选择使用暗器的动作

        //如果配置了动作，则先播放动作
        if (clip != null)
        {
            currentRole.View.PlayAnimation(clip, OnAnimOver, 0.25f);    
        }
        else //否则直接执行逻辑
        {
            OnAnimOver();
        }
    }

    void OnAnimOver()
    {
        currentRole.UseItem(currentItem);
        if (!currentRole.isAI)
        {
            GameRuntimeData.Instance.AddItem(currentItem.Id, -1);
        }
        else
        {
            currentRole.AddItem(Int16.Parse(currentItem.Id), -1);
            Debug.Log(currentItem.Name);
            Debug.Log(currentItem.Id);
        }
        Dictionary<int, int> effects = UIHelper.GetItemEffect(currentItem);
        foreach (var effect in effects)
        {
            if (!GameConst.ProItemDic.ContainsKey(effect.Key.ToString()))
                continue;
            PropertyItem pro = GameConst.ProItemDic[effect.Key.ToString()];
            if (effect.Key == 15 || effect.Key == 17)
            {
                currentRole.View.ShowBattleText($"{pro.Name}+{effect.Value}", Color.blue);
            }
            else if (effect.Key == 6 || effect.Key == 8 || effect.Key == 13 || effect.Key == 16 || effect.Key == 26) 
            {
                currentRole.View.ShowBattleText($"{pro.Name}+{effect.Value}", Color.green);
            }

        }

        Observable.TimerFrame(Convert.ToInt32(castAnimLenght * 60), FrameCountType.FixedUpdate)
            .Subscribe(ms =>
            {
                BattleStateMechine.Instance.SwitchState(BattleManager.BattleViewStates.WaitingForNextActiveBattleRole);
            });
    }

    public override void OnLeaveState()
    {
        currentRole = null;
        currentItem = null;
        BattleStateMechine.Instance.BindItem(null);
    }
}
