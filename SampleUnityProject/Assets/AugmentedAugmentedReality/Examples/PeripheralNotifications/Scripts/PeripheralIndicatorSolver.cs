// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;


public class PeripheralIndicatorSolver : Solver
{
   public Transform DirectionalTarget;

    [Min(0.1f)]
    public float VisibilityScaleFactor = 1.25f;

    [Min(0.0f)]
    public float XViewOffset = 0.3f;
    public float YViewOffset = 0.3f;
    public float ZViewOffset = 0.3f;

    private bool wasVisible = true;

    protected override void Start()
    {
        base.Start();

        SetVisible(IsVisible());
    }

    private void Update()
    {
        bool isVisible = IsVisible();
        if (isVisible != wasVisible)
        {
            SetVisible(isVisible);
        }

        if (isVisible)
        {

        }

    }

    private bool IsVisible()
    {
        if (DirectionalTarget == null || SolverHandler.TransformTarget == null)
        {
            return false;
        }

        return MathUtilities.IsInFOV(DirectionalTarget.position, SolverHandler.TransformTarget,
            VisibilityScaleFactor * CameraCache.Main.fieldOfView, VisibilityScaleFactor * CameraCache.Main.GetHorizontalFieldOfViewDegrees(),
            CameraCache.Main.nearClipPlane, CameraCache.Main.farClipPlane);
    }

    private void SetVisible(bool isVisible)
    {
        SolverHandler.UpdateSolvers = !isVisible;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = !isVisible;
        }

        wasVisible = isVisible;
    }

    /// <inheritdoc />
    public override void SolverUpdate()
    {
        // This is the frame of reference to use when solving for the position of this.gameobject
        // The frame of reference will likely be the main camera
        var solverReferenceFrame = SolverHandler.TransformTarget;

        Vector3 origin = solverReferenceFrame.position + solverReferenceFrame.forward;

        Vector3 trackerToTargetDirection = (DirectionalTarget.position - solverReferenceFrame.position).normalized;

        // Project the vector (from the frame of reference (SolverHandler target) to the Directional Target) onto the "viewable" plane
        Vector3 indicatorDirection = Vector3.ProjectOnPlane(trackerToTargetDirection, -solverReferenceFrame.forward).normalized;

        // If the our indicator direction is 0, set the direction to the right.
        // This will only happen if the frame of reference (SolverHandler target) is facing directly away from the directional target.
        if (indicatorDirection == Vector3.zero)
        {
            indicatorDirection = solverReferenceFrame.right;
        }

        // The final position is translated from the center of the frame of reference plane along the indicator direction vector.
        float start = 0.1f;
        Vector3 offsets = origin + indicatorDirection * start;
        var lPoint = transform.parent.transform.InverseTransformPoint(offsets);
        while (XViewOffset > Mathf.Abs(lPoint.x) && YViewOffset > Mathf.Abs(lPoint.y))
        {
            start += 0.01f;
            offsets = origin + indicatorDirection * start;
            lPoint = transform.parent.transform.InverseTransformPoint(offsets);
        }
        var wPoint = transform.parent.transform.TransformPoint(lPoint);
        GoalPosition = wPoint;

        // Find the rotation from the facing direction to the target object.
        GoalRotation = Quaternion.LookRotation(solverReferenceFrame.forward, indicatorDirection);
    }
}
