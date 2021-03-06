﻿/**
 * $File: JCS_SlideInput.cs $
 * $Date: $
 * $Revision: $
 * $Creator: Jen-Chieh Shen $
 * $Notice: See LICENSE.txt for modification and distribution information 
 *                   Copyright (c) 2016 by Shen, Jen-Chieh $
 */
using UnityEngine;
using System.Collections;

namespace JCSUnity
{
    /// <summary>
    /// Use this to receive the slide input from the device buffer.
    /// </summary>
    public class JCS_SlideInput
        : MonoBehaviour
    {
        /* Variables */

        private Vector2 mDragStartPosition = Vector2.zero;

        [Header("** Check Variables (JCS_SlideInput) **")]

#if (UNITY_STANDALONE || UNITY_EDITOR)
        [Tooltip("Previous position.")]
        [SerializeField]
        private Vector3 mPrePos = Vector3.zero;

        [Tooltip("Is the application currently focused?")]
        [SerializeField]
        private bool mFocus = false;
#endif

        [Tooltip("Is the screen touches?")]
        [SerializeField]
        private bool mTouched = false;

        [Tooltip("Delta value changes on the screen.")]
        [SerializeField]
        private Vector2 mDeltaPos = Vector2.zero;

        [Tooltip("Drag distance.")]
        [SerializeField]
        private Vector2 mDragDistance = Vector2.zero;

        [Tooltip("Drag displacement.")]
        [SerializeField]
        private Vector2 mDragDisplacement = Vector2.zero;

        [Tooltip("Flag to check if drag.")]
        [SerializeField]
        private bool mDragging = false;

#if (UNITY_ANDROID || UNITY_IPHIONE || UNITY_IOS)
        [Tooltip("Flag to check if mult touches.")]
        [SerializeField]
        private bool mMultiTouches = false;

        [Tooltip("Multiple touches distance in average.")]
        [SerializeField]
        private float mTouchDistance = 0.0f;

        [Tooltip("The multi touches distance delta changes per frame.")]
        [SerializeField]
        private float mTouchDistanceDelta = 0.0f;
#endif

#if (UNITY_STANDALONE || UNITY_EDITOR)
        [Header("** Runtime Variables (JCS_SlideInput) **")]

        [Tooltip("Mouse event type of identify the touch event.")]
        [SerializeField]
        private JCS_MouseButton mMouseType = JCS_MouseButton.LEFT;
#endif

#if (UNITY_ANDROID || UNITY_IPHIONE || UNITY_IOS)
        [Tooltip("Touch count that will detect as touched.")]
        [SerializeField]
        [Range(0, 60)]
        private int mDetectTouchCount = 1;
#endif

        /* Setter & Getter */

        public bool Touched { get { return this.mTouched; } }
        public bool Dragging { get { return this.mDragging; } }
        public Vector2 DeltaPos { get { return this.mDeltaPos; } }
        public Vector2 DragDistance { get { return this.mDragDistance; } }
        public Vector2 DragDisplacement { get { return this.mDragDisplacement; } }
#if (UNITY_ANDROID || UNITY_IPHIONE || UNITY_IOS)
        public bool MultiTouches { get { return this.mMultiTouches; } }
        public float TouchDistance { get { return this.mTouchDistance; } }
        public float TouchDistanceDelta { get { return this.mTouchDistanceDelta; } }
        public int DetectTouchCount { get { return this.mDetectTouchCount; } set { this.mDetectTouchCount = value; } }
#else
        public JCS_MouseButton MouseType { get { return this.mMouseType; } set { this.mMouseType = value; } }
#endif

        /* Functions */

        private void Awake()
        {
            JCS_InputManager.instance.SetGlobalSlideInput(this);
        }

        private void Update()
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            mTouched = JCS_Input.GetMouseButton(mMouseType);

            Vector3 currPos = Input.mousePosition;

            // Don't update delta pos when window just focus.
            if (mTouched && !mFocus)
                WhenTouched();
            else
                WhenUntouched();

            mPrePos = currPos;

#elif (UNITY_ANDROID || UNITY_IPHIONE || UNITY_IOS)
            // Detect Touch
            mTouched = (Input.touchCount == mDetectTouchCount);

            if (mTouched)
                WhenTouched();
            else
            {
                WhenUntouched();
                HandleMultiTouches();
            }
#endif
        }

#if (UNITY_STANDALONE || UNITY_EDITOR)
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                mFocus = true;
            }
            else
            {
                // Do something when not focus?
            }
        }
#endif

        /// <summary>
        /// Do thing when is touched.
        /// </summary>
        private void WhenTouched()
        {
            Vector3 currPos = Input.mousePosition;

            if (mDeltaPos == Vector2.zero && mDragDistance == Vector2.zero)
            {
                this.mDragStartPosition = currPos;
            }
            else
            {
                this.mDragging = true;
                Vector2 dragEndPosition = currPos;

                this.mDragDistance.x = JCS_Mathf.DistanceOfUnitVector(mDragStartPosition.x, dragEndPosition.x);
                this.mDragDistance.y = JCS_Mathf.DistanceOfUnitVector(mDragStartPosition.y, dragEndPosition.y);

                float xDiff = dragEndPosition.x - mDragStartPosition.x;
                float yDiff = dragEndPosition.y - mDragStartPosition.y;

                this.mDragDisplacement.x = mDragDistance.x * JCS_Mathf.GetSign(xDiff);
                this.mDragDisplacement.y = mDragDistance.y * JCS_Mathf.GetSign(yDiff);
            }

#if (UNITY_STANDALONE || UNITY_EDITOR)
            mDeltaPos = currPos - mPrePos;
#elif (UNITY_ANDROID || UNITY_IPHIONE || UNITY_IOS)
            mDeltaPos = Input.GetTouch(0).deltaPosition;
#endif
        }

        /// <summary>
        /// Do thing when is not touched.
        /// </summary>
        private void WhenUntouched()
        {
            mDragging = false;
            mDragDistance = Vector2.zero;
            mDragDisplacement = Vector2.zero;

            mDeltaPos = Vector2.zero;

#if (UNITY_STANDALONE || UNITY_EDITOR)
            // If focus, ignore one frame.
            mFocus = false;
#endif
        }

#if (UNITY_ANDROID || UNITY_IPHIONE || UNITY_IOS)
        /// <summary>
        /// Handle multi touches.
        /// </summary>
        private void HandleMultiTouches()
        {
            // Check if multi touches.
            if (Input.touchCount <= 1)
            {
                this.mMultiTouches = false;
                this.mTouchDistance = 0.0f;
                this.mTouchDistanceDelta = 0.0f;
                return;
            }

            float sumTotal = 0.0f;

            for (int index = 1; index < Input.touchCount; ++index)
            {
                var firstTouch = Input.touches[index - 1];
                var currentTouch = Input.touches[index];
                float distance = Vector2.Distance(firstTouch.position, currentTouch.position);
                sumTotal += distance;
            }

            float newTouchDistance = sumTotal / (Input.touchCount - 1);

            // We start apply `delta` value by after the first multi touches.
            if (this.mMultiTouches)
            {
                this.mTouchDistanceDelta = newTouchDistance - this.mTouchDistance;
            }

            this.mTouchDistance = newTouchDistance;

            // Multi touches starts!
            this.mMultiTouches = true;
        }
#endif
    }
}
