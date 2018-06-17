using BattleTech;
using Harmony;
using nl.flukeyfiddler.bt.TurnDirectorDebug.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace nl.flukeyfiddler.bt.TurnDirectorDebug
{
    [HarmonyPatch(typeof(StackManager), "Update")]
    public class StackManager_Update_Patch_Debug
    {
        // TODO have the timer run async, because when a sequence loops, update no longer gets called
        private const long TIMEOUT = 10000;

        private static StackManager stackManager;

        private static IStackSequence previousSequence;
        private static IStackSequence currentSequence;
        private static Stopwatch timer = new Stopwatch();

        public static void Postfix(StackManager __instance)
        {
            Logger.Minimal(".");
            stackManager = __instance;

            List<IStackSequence> sequenceStack = Traverse.Create(__instance).Property("SequenceStack").
                GetValue<List<IStackSequence>>();

            if (sequenceStack.Count > 0 && !shouldIgnoreSequence(sequenceStack[0]))
            {
                currentSequence = sequenceStack[0];
                
                if (isNewSequence())
                {
                    Logger.Minimal(String.Format("SequenceStack count: {0}, curr seq: {1}, Guid: {2}",
                        sequenceStack.Count, currentSequence.GetType(), currentSequence.SequenceGUID));
                    setNewSequenceAndStartTimer();
                    return;
                }

                if (isTimedOut())
                {
                    Logger.Minimal("timed out");
                    fixHangingSequence();
                    resetTimer();
                    return;
                }
            }

            if (sequenceStack.Count == 0)
            {
                if (previousSequence != null)
                {
                    Logger.Minimal("sequenceStack empty");
                    previousSequence = null;
                    startTimer();
                    return;
                }

                if(isTimedOut())
                {
                    Logger.Minimal("sequenceStack empty timeout");
                    fixHangingSequenceStack();
                    resetTimer();
                    return;
                }
            }
        }

        private static void fixHangingSequenceStack()
        {
            AbstractActor failingUnit = removeFailingUnit();
            logHangingSequenceStack(failingUnit);
            
        }

        private static void fixHangingSequence()
        {
            stackManager.RejectStackSequence(currentSequence.SequenceGUID);
            logHangingSequence();
        }

        private static void logHangingSequence()
        {
            List<string> logLines = new List<string>();

            logLines.Add(String.Format("Sequence {0} timed out after {1} ms, type: {2}",
                           previousSequence.SequenceGUID, TIMEOUT, previousSequence.GetType()));
            logLines.Add(String.Format("Active TurnActor: {0}", stackManager.Combat.TurnDirector.ActiveTurnActor.GetType()));

            Logger.Block(logLines.ToArray(), MethodBase.GetCurrentMethod());
        }

        private static void logHangingSequenceStack(AbstractActor failingUnit)
        {
            List<string> logLines = new List<string>();

            logLines.Add(String.Format("Emtpy SequenceStack for {0} ms, ", TIMEOUT));
            logLines.Add(String.Format("Active TurnActor: {0}", stackManager.Combat.TurnDirector.ActiveTurnActor.GetType()));
            logLines.Add(String.Format("Unit name: {0}, Type: {1}, Variant: {2}",
                failingUnit.DisplayName, failingUnit.UnitType, failingUnit.VariantName));
            logLines.Add(String.Format("GUID: {0}, ClassName: {1}", failingUnit.GUID, failingUnit.ClassName));

            Logger.Block(logLines.ToArray(), MethodBase.GetCurrentMethod());
        }

        private static AbstractActor removeFailingUnit()
        {
            List<AbstractActor> units = stackManager.Combat.TurnDirector.ActiveTurnActor.GetUnusedUnitsForCurrentPhase();

            AbstractActor failingUnit = units[0];
            units.Remove(failingUnit);

            return failingUnit;
        }

        private static void startTimer()
        {
            timer.Reset();
            timer.Start();
        }

        private static void resetTimer()
        {
            if(timer.ElapsedMilliseconds > 0)
                timer.Reset();
        }

        private static bool isTimedOut()
        {
            return timer.IsRunning && timer.ElapsedMilliseconds > TIMEOUT;
        }

        private static bool shouldIgnoreSequence(IStackSequence stackSequence)
        {
            bool isTeamActivation = stackSequence.GetType() == typeof(TeamActivationSequence);

            if (isTeamActivation)
            {
                TeamActivationSequence teamSequence = (TeamActivationSequence)stackSequence;
                return teamSequence.team.IsLocalPlayer;
            }

            return (isTeamActivation);
        }

        private static void setNewSequenceAndStartTimer()
        {
            previousSequence = currentSequence;
            timer.Reset();
            timer.Start();
        }

        private static bool isNewSequence()
        {
            return previousSequence == null || previousSequence.SequenceGUID != currentSequence.SequenceGUID;
        }
    }
}
