#region

using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Examples;
using UnityEngine;

#endregion

namespace Enigmaware.AI
{
    public class AdvancedAIMover : AIMoverBase
    {
        
        #region RVO

        [Header("RVOController")] public float ForceMultiplier;
        #endregion
        public bool SetRotation;
        private void TraverseFunnel (RichFunnel fn) {
            // Clamp the current position to the navmesh
            // and update the list of upcoming corners in the path
            // and store that in the 'nextCorners' field
            Vector3 position3D = UpdateTarget(fn);

            // Target point
            steeringTarget = nextCorners[0];
            WishDirection = VectorMath.Normalize(steeringTarget - position3D, out distanceToSteeringTarget);

            if (rvoController != null) {
                rvoController.SetTarget(steeringTarget, 7, distanceToSteeringTarget);
                // this is autistic, but it's needed
                WishDirection = rvoController.CalculateMovementDelta(steeringTarget, Time.deltaTime)*ForceMultiplier;
            }


            if (approachingPartEndpoint)
                if (distanceToSteeringTarget <= endReachedDistance)
                    // Reached the end of the path or an off mesh link
                    NextPart();

            if (SubmitWishDir) {
                
                aiMotor.SetWishDirection(WishDirection);
                if (SetRotation) 
                    aiMotor.Rotation = Quaternion.LookRotation(WishDirection);
            }
        }

        public override void Move()
        {
            RichPathPart currentPart = richPath.GetCurrentPart();

            if (currentPart is RichSpecial)
            {
                // Start traversing the off mesh link if we haven't done it yet
                if (!traversingOffMeshLink && !richPath.CompletedAllParts)
                    StartCoroutine(TraverseSpecial(currentPart as RichSpecial));
            }
            else
            {
                var funnel = currentPart as RichFunnel;

                // Check if we have a valid path to follow and some other script has not stopped the character
                if (funnel != null) //&& !isStopped)
                    TraverseFunnel(funnel);
            }
        }


        #region  Steering Target

        protected readonly List<Vector3> nextCorners = new();

        protected virtual Vector3 UpdateTarget(RichFunnel fn)
        {
            nextCorners.Clear();

            // This method assumes simulatedPosition is up to date as our current position.
            // We read and write to tr.position as few times as possible since doing so
            // is much slower than to read and write from/to a local/member variable.
            bool requiresRepath;
            Vector3 position = fn.Update(transform.position, nextCorners, 2, out lastCorner, out requiresRepath);

            if (requiresRepath && !waitingForPathCalculation && canSearch)
                // TODO: What if canSearch is false? How do we notify other scripts that might be handling the path calculation that a new path needs to be calculated?
                SearchPath();

            return position;
        }

        /// <summary>Distance to <see cref="steeringTarget" /> in the movement plane</summary>
        protected float distanceToSteeringTarget = float.PositiveInfinity;

        protected bool lastCorner;
        
        /// <summary>\copydoc Pathfinding::IAstarAI::steeringTarget</summary>
        public Vector3 steeringTarget { get; protected set; }

        #endregion
        
        #region Built-in Methods

        public void Update()
        {
            
            if (!reachedDestination)
            {
                if (shouldRecalculatePath)
                {
                    SearchPath();
                }

                Move();
            }
            else
            {
                WishDirection = Vector3.zero;
                aiMotor.SetWishDirection(WishDirection);
            }
        }

        public bool SubmitWishDir = true;
        
        public void OnEnable()
        {
            seeker.pathCallback += OnPathComplete;
        }

        public void OnDisable()
        {
            seeker.pathCallback -= OnPathComplete;
        }
        #endregion
        
        #region Links

        /// <summary>Traverses an off-mesh link</summary>
        protected virtual IEnumerator TraverseSpecial(RichSpecial link)
        {
            traversingOffMeshLink = true;
            // The current path part is a special part, for example a link
            // Movement during this part of the path is handled by the TraverseSpecial coroutine
            var offMeshLinkCoroutine = onTraverseOffMeshLink != null
                ? onTraverseOffMeshLink(link)
                : TraverseOffMeshLinkFallback(link);
            yield return StartCoroutine(offMeshLinkCoroutine);

            // Off-mesh link traversal completed
            traversingOffMeshLink = false;
            NextPart();

            // If a path completed during the time we traversed the special connection, we need to recalculate it
            if (delayUpdatePath)
            {
                delayUpdatePath = false;
                // TODO: What if canSearch is false? How do we notify other scripts that might be handling the path calculation that a new path needs to be calculated?
                SearchPath();
            }
        }
        
        /// <summary>
        ///     Fallback for traversing off-mesh links in case <see cref="onTraverseOffMeshLink" /> is not set.
        ///     This will do a simple linear interpolation along the link.
        /// </summary>
        protected IEnumerator TraverseOffMeshLinkFallback(RichSpecial link)
        {
            /*float duration = maxSpeed > 0 ? Vector3.Distance(link.second.position, link.first.position) / maxSpeed : 1;
            float startTime = Time.time;

            while (true) {
                var pos = Vector3.Lerp(link.first.position, link.second.position, Mathf.InverseLerp(startTime, startTime + duration, Time.time));
                if (updatePosition) tr.position = pos;
                else simulatedPosition = pos;

                if (Time.time >= startTime + duration) break;
                yield return null;
            }*/
            yield return null;
        }
    
        #endregion

        #region Parts

        /// <summary>
        ///     True if approaching the last wqaypoint in the current part of the path.
        ///     Path parts are separated by off-mesh links.
        ///     See: <see cref="approachingPathEndpoint" />
        /// </summary>
        public bool approachingPartEndpoint => lastCorner && nextCorners.Count == 1;

        /// <summary>
        ///     Declare that the AI has completely traversed the current part.
        ///     This will skip to the next part, or call OnTargetReached if this was the last part
        /// </summary>
        protected void NextPart()
        {
            if (!richPath.CompletedAllParts)
            {
                if (!richPath.IsLastPart) lastCorner = false;
                richPath.NextPart();
                if (richPath.CompletedAllParts) OnTargetReached();
            }
        }
        
        #endregion

        #region Off mesh links

        /// <summary>
        ///     Called when the agent starts to traverse an off-mesh link.
        ///     Register to this callback to handle off-mesh links in a custom way.
        ///     If this event is set to null then the agent will fall back to traversing
        ///     off-mesh links using a very simple linear interpolation.
        ///     <code>
        /// void OnEnable () {
        ///     ai = GetComponent<RichAI>
        ///             ();
        ///             if (ai != null) ai.onTraverseOffMeshLink += TraverseOffMeshLink;
        ///             }
        ///             void OnDisable () {
        ///             if (ai != null) ai.onTraverseOffMeshLink -= TraverseOffMeshLink;
        ///             }
        ///             IEnumerator TraverseOffMeshLink (RichSpecial link) {
        ///             // Traverse the link over 1 second
        ///             float startTime = Time.time;
        ///             while (Time.time
        ///             < startTime + 1) {
        ///                 transform.position= Vector3.Lerp( link.first.position, link.second.position, Time.time - startTime);
        ///                 yield return null;
        ///                 }
        ///                 transform.position= link.second.position;
        /// }
        /// </code>
        /// </summary>
        public Func<RichSpecial, IEnumerator> onTraverseOffMeshLink;
        public bool traversingOffMeshLink { get; protected set; }

        #endregion
        
        #region Path handling

        [Header("Pathfinding")]

        [Tooltip("The distance to the end point to consider the end of path to be reached")]
        
        public float endReachedDistance = 3f;

        /// <summary>
        ///     Determines how the agent recalculates its path automatically.
        ///     This corresponds to the settings under the "Recalculate Paths Automatically" field in the inspector.
        /// </summary>
        public AutoRepathPolicy autoRepath = new();
        
        /// <summary>Holds the current path that this agent is following</summary>
        protected readonly RichPath richPath = new();
        
        protected bool delayUpdatePath;
        protected bool waitingForPathCalculation;

        /// <summary>
        ///     Use funnel simplification.
        ///     On tiled navmesh maps, but sometimes on normal ones as well, it can be good to simplify
        ///     the funnel as a post-processing step to make the paths straighter.
        ///     This has a moderate performance impact during frames when a path calculation is completed.
        ///     The RichAI script uses its own internal funnel algorithm, so you never
        ///     need to attach the FunnelModifier component.
        ///     [Open online documentation to see images]
        ///     See: <see cref="Pathfinding.FunnelModifier" />
        /// </summary>
        [Tooltip("Should a FunnelModifier be used on the path")]
        public bool funnelSimplification;

        [Tooltip("Can the agent search for new paths. Disable this if you want to handle path requests manually.")]
        public bool canSearch = true;
        
        /// <summary>
        ///     True if approaching the last waypoint of all parts in the current path.
        ///     Path parts are separated by off-mesh links.
        ///     See: <see cref="approachingPartEndpoint" />
        /// </summary>
        public bool approachingPathEndpoint => approachingPartEndpoint && richPath.IsLastPart;

        /// <summary>
        ///     Outputs the start point and end point of the next automatic path request.
        ///     This is a separate method to make it easy for subclasses to swap out the endpoints
        ///     of path requests. For example the <see cref="LocalSpaceRichAI" /> script which requires the endpoints
        ///     to be transformed to graph space first.
        /// </summary>
        protected virtual void CalculatePathRequestEndpoints(out Vector3 start, out Vector3 end)
        {
            start = transform.position;
            end = destination;
        }
        
        public bool reachedEndOfPath => approachingPathEndpoint && distanceToSteeringTarget < endReachedDistance;

        /// <summary>True if the path should be automatically recalculated as soon as possible</summary>
        protected virtual bool shouldRecalculatePath =>
            autoRepath.ShouldRecalculatePath(transform.position, aiMotor.Motor.Capsule.radius,
                destination) && !traversingOffMeshLink;

        /// <summary>\copydoc Pathfinding::IAstarAI::reachedDestination</summary>
        public override bool reachedDestination
        {
            get
            {
                if (!reachedEndOfPath) return false;
                // Note: distanceToSteeringTarget is the distance to the end of the path when approachingPathEndpoint is true
                if (approachingPathEndpoint && distanceToSteeringTarget + (destination - richPath.Endpoint).magnitude >
                    endReachedDistance) return false;
                return true;
            }
        }

        public override bool hasPath => richPath.GetCurrentPart() != null;
        public override bool pathPending => waitingForPathCalculation || delayUpdatePath;

        public override void SearchPath()
        {
            // Calculate paths after the current off-mesh link has been completed
            if (traversingOffMeshLink)
            {
                delayUpdatePath = true;
            }
            else
            {
                if (float.IsPositiveInfinity(destination.x)) return;
                //todo: implement event for when the AI searches a path 

                base.SearchPath();
                
                Vector3 start, end;
                CalculatePathRequestEndpoints(out start, out end);

                // Request a path to be calculated from our current position to the destination
                ABPath p = ABPath.Construct(start, end);
                SetPath(p, false);
            }
        }

        public void SetPath(Path path, bool updateDestinationFromPath = true)
        {
            if (updateDestinationFromPath && path is ABPath abPath && !(path is RandomPath))
                destination = abPath.originalEndPoint;

            if (path == null)
            {
                CancelCurrentPathRequest();
                ClearPath();
            }
            else if (path.PipelineState == PathState.Created)
            {
                // Path has not started calculation yet
                waitingForPathCalculation = true;
                seeker.CancelCurrentPathRequest();
                seeker.StartPath(path);
                autoRepath.DidRecalculatePath(destination);
            }
            else if (path.PipelineState == PathState.Returned)
            {
                // Path has already been calculated

                // We might be calculating another path at the same time, and we don't want that path to override this one. So cancel it.
                if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();
                else
                    throw new ArgumentException(
                        "If you calculate the path using seeker.StartPath then this script will pick up the calculated path anyway as it listens for all paths the Seeker finishes calculating. You should not call SetPath in that case.");

                OnPathComplete(path);
            }
            else
            {
                // Path calculation has been started, but it is not yet complete. Cannot really handle this.
                throw new ArgumentException(
                    "You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
            }
        }

        protected void ClearPath()
        {
            CancelCurrentPathRequest();
            richPath.Clear();
            lastCorner = false;
            delayUpdatePath = false;
            distanceToSteeringTarget = float.PositiveInfinity;
        }

        protected void CancelCurrentPathRequest()
        {
            waitingForPathCalculation = false;
            // Abort calculation of the current path
            if (seeker != null) seeker.CancelCurrentPathRequest();
        }

        public void OnPathComplete(Path p)
        {
            waitingForPathCalculation = false;
            p.Claim(this);

            if (p.error)
            {
                p.Release(this);
                return;
            }

            if (traversingOffMeshLink)
            {
                delayUpdatePath = true;
            }
            else
            {
                // The RandomPath and MultiTargetPath do not have a well defined destination that could have been
                // set before the paths were calculated. So we instead set the destination here so that some properties
                // like #reachedDestination and #remainingDistance work correctly.
                if (p is RandomPath rpath)
                    destination = rpath.originalEndPoint;
                else if (p is MultiTargetPath mpath) destination = mpath.originalEndPoint;

                richPath.Initialize(seeker, p, true, funnelSimplification);

                // Check if we have already reached the end of the path
                // We need to do this here to make sure that the #reachedEndOfPath
                // property is up to date.
                var part = richPath.GetCurrentPart() as RichFunnel;
                if (part != null)
                {
                    // Note: UpdateTarget has some side effects like setting the nextCorners list and the lastCorner field
                    var localPosition = UpdateTarget(part);

                    // Target point
                    steeringTarget = nextCorners[0];
                    distanceToSteeringTarget = (steeringTarget - localPosition).magnitude;

                    if (lastCorner && nextCorners.Count == 1 && distanceToSteeringTarget <= endReachedDistance)
                        NextPart();
                }
            }

            p.Release(this);
        }

        #endregion
        
        #region Misc

        /// <summary>\copydoc Pathfinding::IAstarAI::remainingDistance</summary>
        public float remainingDistance
        {
            get { return distanceToSteeringTarget + Vector3.Distance(steeringTarget, richPath.Endpoint); }
        }
        
        #endregion
    }
}