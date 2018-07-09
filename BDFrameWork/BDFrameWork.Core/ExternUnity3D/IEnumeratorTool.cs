﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
public class IEnumeratorTool : MonoBehaviour
{
    /// <summary>
    /// 压入的action任务
    /// </summary>
    public class ActionTask
    {
        public Action WillDoAction;
        public Action CallBackAction;
    }
    /// <summary>
    /// 任务队列
    /// </summary>
    static Queue<ActionTask> actionTaskQueue = new Queue<ActionTask>();

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="action"></param>
    /// <param name="callBack"></param>
    static public void ExecAction(Action action,Action callBack =null)
    {
        var task = new ActionTask()
        {
            WillDoAction = action,
            CallBackAction = callBack
        };

        
        actionTaskQueue.Enqueue(task);
    }


    /// <summary>
    /// 立即执行 任务队列
    /// </summary>
    static Queue<ActionTask> actionTaskQueueImmediately = new Queue<ActionTask>();
    /// <summary>
    /// 立即执行
    /// </summary>

    static public void ExecActionImmediately(Action action, Action callBack = null)
    {
        var task = new ActionTask()
        {
            WillDoAction = action,
            CallBackAction = callBack
        };


        actionTaskQueueImmediately.Enqueue(task);
    }

    //
    static Dictionary<int, IEnumerator> IEnumeratorDictionary = new Dictionary<int, IEnumerator>();
    static Dictionary<int, Coroutine> CoroutineDictionary = new Dictionary<int, Coroutine>();
    static Queue<int> IEnumeratorQueue = new Queue<int>();
    static int counter = -1;
    static public  int IStartCoroutine (IEnumerator ie)
    {
        counter++;
        IEnumeratorQueue.Enqueue(counter);
        IEnumeratorDictionary[counter] = ie;
        return counter;
    }

    static Queue<int> StopIEIdQueue = new Queue<int>();
    static public void IStopCoroutine(int id)
    {
        StopIEIdQueue.Enqueue(id);
    }
       
   static private bool isStopAllCoroutine = false;
   /// <summary>
   /// 停止携程
   /// </summary>
    static public void StopAllCroutine()
    {
        isStopAllCoroutine = true;
    }

#region Tools

    /// <summary>
    /// 等待一段时间后执行
    /// </summary>
    /// <param name="f"></param>
    /// <param name="action"></param>
   static public void WaitingForExec(float f, Action action)
    {
        IStartCoroutine(IE_WaitingForExec(f, action));
    }

  static  private IEnumerator IE_WaitingForExec(float f, Action action)
    {
        yield return new WaitForSeconds(f);
        if(action!=null)
          action();
        yield break;
    }
    #endregion
    /// <summary>
    /// 主循环
    /// </summary>
    void Update()
    {
        //停止所有携程
        if (isStopAllCoroutine) {
             BDebug.Log("停止所有携程");
            StopAllCoroutines();
            isStopAllCoroutine = false;
        }
        //优先停止携程
        while (StopIEIdQueue.Count > 0) {

            var id = StopIEIdQueue.Dequeue();
            if (CoroutineDictionary.ContainsKey(id)) {
                var coroutine = CoroutineDictionary[id];
                base.StopCoroutine(coroutine);
                //
                CoroutineDictionary.Remove(id);
            } else {
                Debug.LogErrorFormat("此id协程不存在,无法停止:{0}", id);
            }
        }

        //携程循环
        if (IEnumeratorQueue.Count > 0) {
            var id = IEnumeratorQueue.Dequeue();
            //取出携程
            var ie = IEnumeratorDictionary[id];
            IEnumeratorDictionary.Remove(id);
            //执行携程
            var coroutine = base.StartCoroutine(ie);

            //存入coroutine
            CoroutineDictionary[id] = coroutine;
        }

        //主线程循环 立即执行
        while (actionTaskQueueImmediately.Count > 0) {

            var task = actionTaskQueueImmediately.Dequeue();
            task.WillDoAction();
            if (task.CallBackAction != null) {
                task.CallBackAction();
            }
        }

        //主线程循环
        if (actionTaskQueue.Count > 0) {

            var task = actionTaskQueue.Dequeue();
            task.WillDoAction();
            if (task.CallBackAction != null) {
                task.CallBackAction();
            }
        }
    }

}
