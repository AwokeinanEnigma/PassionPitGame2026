using UnityEngine;
using System.Collections.Generic;

namespace Enigmaware.Gravity
{
    public class GravityFinder
    {

        public struct GravityInformation
        {
            public Vector3 gravity;
            public Vector3 upAxis;
            public GravitySource[] sources;
            
        }
        
        public static List<GravitySource> sources = new List<GravitySource>();

        public static void Register(GravitySource source)
        {
            Debug.Assert(
                !sources.Contains(source),
                "Duplicate registration of gravity source!", source
            );
            sources.Add(source);
        }

        public static void Unregister(GravitySource source)
        {
            Debug.Assert(
                sources.Contains(source),
                "Unregistration of unknown gravity source!", source
            );
            sources.Remove(source);
        }

        public static Vector3 GetGravity(Vector3 position)
        {
            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }

            return g;
        }

        public static List<GravitySource> cleared = new List<GravitySource>();
        

        public static GravityInformation GetGravityInformation(Vector3 position)
        {
            cleared.Clear();
            
            Vector3 g = Vector3.zero;
            GravityInformation info = new GravityInformation();
            
            for (int i = 0; i < sources.Count; i++)
            {
                Vector3 force = sources[i].GetGravity(position);
                g += force;
                
                if (force != Vector3.zero)
                {
                    cleared.Add(sources[i]);
                }
            }
            info.sources = cleared.ToArray();
            info.gravity = g;
            info.upAxis = -g.normalized;
            return info;
        }

        public static Vector3 GetUpAxis(Vector3 position)
        {
            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }

            return -g.normalized;
        }
    }
}