using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Packrats
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class BuildingCard : MonoBehaviour
    {
        public const string DEFAULT_BUILDING_MATERIA_NAME = "BuildingCardMaterial";

        private static Material _cardMaterial;

        public Texture CardTexture;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;


        public float CardHeight = 2.0f;
        public float CardStartRadians = 0.0f;
        public float CardEndRadians = 1.0f;
        public float CardDepth = 5.0f;

        public float CardLengthRadians => CardEndRadians - CardStartRadians;

        private Mesh _gizmoMesh;

        void Awake()
        {
            FindComponents();
            _meshRenderer.sharedMaterial = null;
            SetupMaterial();
            var caveSettings = CaveSystemSettings.DefaultSettings;
            RemeshCard(ref caveSettings);
        }

        private void FindComponents()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void SetupMaterial()
        {
            FindComponents();

            Material cardMaterial = GetCardMaterial();
            if (_meshRenderer.sharedMaterial == null)
            {
                cardMaterial = GetCardMaterial();
                _meshRenderer.sharedMaterial = cardMaterial;
            }
            else
            {
                cardMaterial = _meshRenderer.sharedMaterial;
            }
            cardMaterial.mainTexture = CardTexture;
        }

        private Material GetCardMaterial()
        {
            if (_cardMaterial == null)
            {
                _cardMaterial = Resources.Load<Material>(DEFAULT_BUILDING_MATERIA_NAME);
            }

            var copy = Instantiate(_cardMaterial);
            copy.CopyPropertiesFromMaterial(_cardMaterial);
            return copy;
        }

        //Generally only called though the editor, but could be useful for some other cases as well
        
        /// <summary>
        /// Used by the editor when the texture changes, re-calculates the size of the card
        /// </summary>
        public void OnTextureChanged(CaveSystemSettings caveSettings)
        {
            SetupMaterial();

            CardHeight = caveSettings.RoomHeight;


        } 

        public void OnEdited()
        {
            FindComponents();

            var caveSettings = CaveSystemSettings.DefaultSettings;

            float height = transform.localPosition.y;

            //Compute position relative to the cave center, in polar cords
            float3 centerRelative = (float3)transform.localPosition + new float3(caveSettings.InnerRadius, 0.0f, 0.0f);
            centerRelative.y = 0.0f;

            float radial = math.atan2(centerRelative.z, centerRelative.x);
            float distance = math.length(centerRelative);
            CardDepth = distance;

            float2 desiredCardSize = GetCardSizeFromTextureAndRoomHeight(CardTexture, CardHeight);

            float cardLenghtInRadians = desiredCardSize.x / distance;

            CardStartRadians = radial;
            CardEndRadians = radial + cardLenghtInRadians;

            RemeshCard(ref caveSettings);
        }

        private Mesh RemeshCard(ref CaveSystemSettings caveSettings)
        {
            var nativeMesh = new NativeMeshData(Allocator.TempJob);

            int segments = (int)(math.ceil(2.0f * CardLengthRadians / caveSettings.SegmentSizeRadians));

            CylinderMesher.AppendWalls(ref nativeMesh, segments, CardStartRadians, CardLengthRadians, CardDepth, 0.0f, CardHeight);

            var localPosition = transform.localPosition;
            localPosition.y = 0.0f;

            float4x4 transformMatrix = Matrix4x4.Translate(new Vector3(-caveSettings.InnerRadius, 0.0f, 0.0f) - localPosition);
            NativeMeshTransformer.TransformMesh(ref nativeMesh, ref transformMatrix, default).Complete();

            var mesh = new Mesh();
            nativeMesh.ApplyToMesh(mesh);
            nativeMesh.Dispose();

            _gizmoMesh = mesh;
            _meshFilter.mesh = _gizmoMesh;

            return mesh;
        }

        private float2 GetCardSizeFromTextureAndRoomHeight(Texture texture, float roomHeight)
        {
            if (texture == null)
            {
                return new float2(roomHeight, roomHeight);
            }
            float widthPerHeight = (float)texture.width / texture.height;
            return new float2(widthPerHeight * roomHeight, roomHeight);
        }

        public void OnCardMoved()
        {

        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(_gizmoMesh, transform.position, transform.parent.rotation);
        }

        //How does this get set ???
        //How does this get it's bendy args
        //How is this built in the editor, in a way that makes it, not a nightmare

        //Base this on the center position of the card ?????, doing position based on percentage, or radians

    }
}
