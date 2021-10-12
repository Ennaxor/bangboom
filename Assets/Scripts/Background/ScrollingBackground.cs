using UnityEngine;

namespace BangBoom.Background
{
	public class ScrollingBackground : MonoBehaviour
	{
		[SerializeField] private MeshRenderer mesh;
		[SerializeField] private float speed;

		[HideInInspector] public GameObject Player;

		private void Update()
		{
			var material = mesh.material;
			var offset = material.mainTextureOffset;
			var position = Player.transform.position;
			var localScale = transform.localScale;
			offset.x = position.x / localScale.y / speed;
			offset.y = position.y / localScale.y / speed;
			material.mainTextureOffset = offset;
		}
	}
}