﻿using UnityEngine;
using System.Collections;

namespace TrollBridge {

	public class Icon_Display : MonoBehaviour {

		/// The GameObject to appear as an icon.
		public GameObject Icon;
		/// The initial location of the icon.
		public Transform iconLocation;

		/// Set to 'true' if you want the icon's movement to bounce.
		public bool BounceIcon;
		public float BounceDistance = 0.2f;
		public float BounceTime = 0.75f;

		/// Set to 'true' if you want the icon's movment to pulse.
		public bool PulseIcon;
		public float PulseIntensity = 1.3f;
		public float PulseTime = 0.75f;

		/// Set to 'true' if you want the icon's movement to spin.
		public bool SpinIcon;
		public float SpinXSpeed = 0f;
		public float SpinYSpeed = 2f;

		private GameObject _icon;
		private Transform _iconTransform;


		void Start(){
			// Check to make sure the user has the scripts working correctly.
			DebugCheck();
		}

		void DebugCheck(){
			// IF the user did not set an Icon.
			if(Icon == null){
				Grid.helper.DebugErrorCheck(75, this.GetType(), gameObject);
			}
		}

		void OnTriggerEnter2D(Collider2D coll){
			// IF the colliding object's tag isn't Player.
			if(coll.tag != "Player"){
				return;
			}

			// Create the icon.
			_icon = Instantiate(Icon, iconLocation.position, Quaternion.identity) as GameObject;
			// Set the icon transform.
			_iconTransform = _icon.transform;
			// Set the parent.
			_iconTransform.SetParent(gameObject.transform);

			// IF we want the icon to bounce.
			if(BounceIcon){
				StartCoroutine(BouncingIcon(BounceTime));
			}
			// IF we want the icon to pulse.
			if(PulseIcon){
				StartCoroutine(PulsingIcon(_iconTransform.localScale,
				                           _iconTransform.localScale * PulseIntensity, PulseTime));
			}
			// IF we want the icon to spin.
			if(SpinIcon){
				StartCoroutine(SpinningIcon(SpinXSpeed, SpinYSpeed));
			}
		}

		void OnTriggerExit2D(Collider2D coll){
			// IF the colliding object's tag isn't Player.
			if(coll.tag != "Player"){
				return;
			}
			// Stop all the coroutines.
			StopAllCoroutines();
			// Destroy the icon.
			Destroy(_icon);
		}

		private IEnumerator BouncingIcon(float bounceTime){
			while(true){
				// Move from Point A to Point B.
				yield return StartCoroutine(MoveGameObject(bounceTime, true));
				// Move from Point B to Point A.
				yield return StartCoroutine(MoveGameObject(bounceTime, false));
			}
		}

		private IEnumerator MoveGameObject(float time, bool start){
			// IF this is the start of the movement,
			// ELSE we are moving back to the first spot.
			if(start){
				for(float i = 0f; i < 1.0f; i += Time.deltaTime/time){
					// Lerp the icon to the From position to the To position.
					_iconTransform.position = Vector2.Lerp(iconLocation.position, new Vector2(iconLocation.transform.position.x, iconLocation.transform.position.y + BounceDistance), i);
					yield return null;
				}
			}else{
				for(float i = 0f; i < 1.0f; i += Time.deltaTime/time){
					// Lerp the icon to the From position to the To position.
					_iconTransform.position = Vector2.Lerp(new Vector2(iconLocation.transform.position.x, iconLocation.transform.position.y + BounceDistance), iconLocation.position, i);
					yield return null;
				}
			}
		}
		
		private IEnumerator PulsingIcon(Vector2 from, Vector2 to, float pulseTime){
			while(true){
				// Scale from size A to size B.
				yield return StartCoroutine(PulseGameObject(from, to, pulseTime));
				// Scale from size B to size A.
				yield return StartCoroutine(PulseGameObject(to, from, pulseTime));
			}
		}
		
		private IEnumerator PulseGameObject(Vector2 from, Vector2 to, float time){
			for(float i = 0f; i < 1.0f; i += Time.deltaTime/time){
				// Lerp the icon to the From scale to the To scale
				_iconTransform.localScale = Vector2.Lerp(from, to, i);
				yield return null;
			}
		}


		private IEnumerator SpinningIcon(float xSpin, float ySpin){
			while(true){
				// Rotate the icon.
				_iconTransform.Rotate(xSpin, ySpin, 0f);
				yield return null;
			}
		}
	}
}
