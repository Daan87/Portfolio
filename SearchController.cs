using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BritannicaAPP
{
	public class SearchController: MonoBehaviour
	{
		public List<Object> Shards;
		public List<Material> Materials;

		private GameObject SearchSphere;
		private Camera Camera;
		private bool StartSearch;

		public Collectible Collectible
		{
			get;
			set;
		}

		// Use this for initialization
		void Start()
		{
			SearchSphere = GameObject.Find("SearchArea/Sphere");
			Camera = GameObject.Find("Main Camera").GetComponent<Camera>();
			SearchSphere.SetActive(false);
		}

		// Update is called once per frame
		void Update()
		{
			if(!StartSearch)
			{
				return;
			}

			Quaternion rotFix = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);
			Camera.transform.localRotation = Quaternion.Slerp(Camera.transform.localRotation, rotFix, .275f);

			foreach(Touch touch in Input.touches)
			{
				if(touch.phase == TouchPhase.Began)
				{
					// Construct a ray from the current touch coordinates
					Ray ray = Camera.main.ScreenPointToRay(touch.position);
					if(Physics.Raycast(ray))
					{
						GlobalController.Instance.FadeController.FadeOut(CollectibleFound);
					}
				}
			}
		}

		public void StartSearching()
		{
			GenerateCollectible();
			SearchSphere.SetActive(true);
			StartSearch = true;
		}

		public void Reset()
		{
			RemoveCollectible();
			Input.gyro.enabled = false;
			SearchSphere.SetActive(false);
			StartSearch = false;

			Camera.transform.localRotation = new Quaternion(0,0,0,0);
		}

		private void CollectibleFound()
		{
			GlobalController.Instance.CollectibleControl.SetCollectibleFound(Collectible.CollectibleType);
			GlobalController.Instance.CollectibleControl.SaveCollectibles();
			GlobalController.Instance.CollectibleControl.CollectibleViewReturnAction = GlobalController.Instance.ViewControl.StartScan;
			GlobalController.Instance.StateControl.SwitchState(AppStates.SHOWING);
			Reset();
		}

		private void RemoveCollectible()
		{
			foreach(Transform obj in SearchSphere.transform)
			{
				Destroy(obj.gameObject);
			}
		}

		private void GenerateCollectible()
		{
			Vector3 collectiblePosition = GetRandomPosition();

			GameObject collectible = Instantiate(GetRandomShard(), collectiblePosition, Quaternion.identity) as GameObject;
			collectible.GetComponentInChildren<MeshRenderer>().material = GetRandomMaterial();
			collectible.transform.parent = SearchSphere.transform;
			collectible.transform.LookAt(SearchSphere.transform.position);
			collectible.transform.Rotate(-90, 0, 0);
		}

		private Object GetRandomShard()
		{
			System.Random rand = new System.Random();
			int index = rand.Next(Shards.Count);

			return Shards[index];
		}

		private Material GetRandomMaterial()
		{
			System.Random rand = new System.Random();
			int index = rand.Next(Materials.Count);

			return Materials[index];
		}

		private Vector3 GetRandomPosition()
		{
			Vector3 position = new Vector3();

			float radius = SearchSphere.GetComponent<Renderer>().bounds.extents.magnitude;

			position = Random.onUnitSphere * (radius * 0.5f) + SearchSphere.transform.position;

			while(position.z <= -0.3f)
			{
				position = Random.onUnitSphere * (radius * 0.5f) + SearchSphere.transform.position;
			}

			return position;
		}
	}
}
