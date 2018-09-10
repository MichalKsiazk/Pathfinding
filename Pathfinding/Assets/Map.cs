using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Map : MonoBehaviour {

	public enum TileType {
		Solid, Void, Destination, Start
	}
		
	public int width, height;

	[SerializeField]
	public TileInfo[] tileMap;
	public GameObject background;
	public GameObject tile;
	public TileType tileType;

	[HideInInspector]
	public int moveSpeed;

	Coroutine animator;

	Step[,] pathMap;


	public Vector2 start, end;

	[HideInInspector]
	public Color solidColor, destinationColor, startColor, voidColor, pathColor;

	GameObject cam;

	public List<Step> path;

	public int stepCount = 0;
	public int timer = 0;


	void Start () {
		//List<Step> path = FindPath ();
		//DrawPath (path);

	}

	public void UpdateInEditMode() {
		timer++;
		if (timer >= moveSpeed)
			timer = 0;
		if (stepCount > 0 && timer == 0 && path != null) {
			Move (path, stepCount);
			stepCount--;
		}
	}


	public void Reset() {

		voidColor = tile.GetComponent<SpriteRenderer> ().color;
		Transform[] allChildren = GetComponentsInChildren<Transform>();

		foreach (Transform child in allChildren) {
			if (child.gameObject.tag == "tile")				
				DestroyImmediate (child.gameObject);
		}
		tileMap = null;
		tileMap = new TileInfo[width * height];
	}

	public void Generate() {
		Reset ();
		cam = Camera.main.gameObject;
		background.transform.localScale = new Vector2 (width + 0.2f, height + 0.2f);
		background.transform.position = new Vector3 (width/2 - 0.5f, height/2 - 0.5f, 1);
		cam.transform.position = new Vector3 (width / 2 - 0.5f, height / 2 - 0.5f, cam.transform.position.z);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tileMap[width * y + x] = new TileInfo(Instantiate (tile, new Vector2 (x, y), Quaternion.identity),0,x,y);
				tileMap[width * y + x].tile.transform.parent = this.transform;

			}
		}
	}

	public void Select(Vector3 mousePos) {
		int fx = Mathf.FloorToInt (mousePos.x + 0.5f);
		int fy = Mathf.FloorToInt (mousePos.y + 0.5f);
		if(fx >= 0 && fx < width && fy >= 0 && fy < height)
			Repaint(tileMap[fx + width * fy]);
	}

	public void Repaint(TileInfo tileToRepaint) {
		switch (tileType) {
		case TileType.Destination:
			if(end != null){
				tileMap [(int)end.x + width * (int)end.y].tile.GetComponent<SpriteRenderer> ().color = voidColor;
				tileMap [(int)end.x + width * (int)end.y].type = 0;
			}
			tileToRepaint.tile.GetComponent<SpriteRenderer> ().color = destinationColor;
			tileToRepaint.type = 4;
			end = new Vector2 (tileToRepaint.x, tileToRepaint.y);
			break;
		case TileType.Solid:
			tileToRepaint.tile.GetComponent<SpriteRenderer> ().color = solidColor;
			tileToRepaint.type = 1;
			break;
		case TileType.Start:
			if(start != null){
				tileMap [(int)start.x + width * (int)start.y].tile.GetComponent<SpriteRenderer> ().color = voidColor;
				tileMap [(int)start.x + width * (int)start.y].type = 0;
			}
			tileToRepaint.tile.GetComponent<SpriteRenderer> ().color = startColor;
			tileToRepaint.type = 3;
			start = new Vector2 (tileToRepaint.x, tileToRepaint.y);
			break;
		case TileType.Void:
			tileToRepaint.tile.GetComponent<SpriteRenderer> ().color = voidColor;
			tileToRepaint.type = 0;
			break;
		}
	}

	public List<Step> FindPath() {

		pathMap = null;
		pathMap = new Step [width, height];

		int count = 0;

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				pathMap [x, y] = new Step (new Vector2(), new Vector2(), 0);
			}
		}
		count++;

		int sx = (int)start.x;
		int sy = (int)start.y;

		pathMap [sx, sy] = new Step (new Vector2(sx, sy), start, 1);

		if (sx >= 1) pathMap [sx - 1, sy] = new Step (new Vector2 (sx - 1, sy), start, count);
		if (sx < width - 1) pathMap [sx + 1, sy] = new Step (new Vector2 (sx + 1, sy), start, count);
		if (sy >= 1) pathMap [sx, sy - 1] = new Step (new Vector2 (sx, sy - 1), start, count);
		if (sy < height - 1) pathMap [sx, sy + 1] = new Step (new Vector2 (sx, sy + 1), start, count);

		while (true) {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (pathMap[x,y].count == count) {
						Step nextStep = SetTiles (pathMap[x,y]);
						if (nextStep != null)
							return MakePath (nextStep);
					}
				}
			}	
			count++;
			if (count > width * height)
				break;
		}
		return null;
	}

	List<Step> MakePath(Step step) {
		List<Step> path = new List<Step> ();
		path.Add (step);

		Step lastStep = pathMap[(int)step.fromWhere.x, (int)step.fromWhere.y];
		int c = step.count;
		while(c > 0){
			path.Add (lastStep);
			lastStep = pathMap [(int)lastStep.fromWhere.x, (int)lastStep.fromWhere.y];
			c--;
		}
		return path;
	}


	Step SetTiles(Step step) {

		int sx = (int)step.pos.x;
		int sy = (int)step.pos.y;

		for (int x = -1; x <= 1; x++) {
			if (x == 0)
				continue;
			

			if (sx + x < 0 || sx + x >= width || pathMap [sx + x, sy] == null)
				continue;

			if (pathMap [sx + x, sy].count == 0 && sx + x >= 0 && sx + x < width && sx + x < width && sx + x >= 0) {
				if (tileMap [width * sy + sx + x].type != 1) {
					pathMap [sx + x, sy] = new Step (new Vector2 (sx + x, sy), step.pos, step.count + 1);
					tileMap [width * sy + sx + x].type = 5;
					if (pathMap [sx + x, sy].pos == end) {
						tileMap [width * sy + sx + x].type = 0;
						return pathMap [sx + x, sy];
					}
				}
			}
		}

		for (int y = -1; y <= 1; y++) {
			if (y == 0)
				continue;

			int linearPos = width * sy + sx + (width * y);

			if (sy + y < 0 || sy + y >= height || pathMap [sx , sy + y] == null)
				continue;

			if (pathMap [sx, sy + y].count == 0 && sy + y >= 0 && sy + y < height && linearPos < width * height && linearPos >= 0) {
				if (tileMap [linearPos].type != 1) {
					tileMap [linearPos].type = 5;
					pathMap [sx, sy + y] = new Step (new Vector2 (sx, sy + y), step.pos, step.count + 1);
					if (pathMap [sx, sy + y].pos == end) {
						tileMap [linearPos].type = 0;
						return pathMap [sx, sy + y];
					}
				}
			}
		}
		return null;
	}

	public void DrawPath(List<Step> path) {
		foreach (Step s in path) {
			if (s.pos == end || s.pos == start)
				continue;
			tileMap [(int)s.pos.x + width * (int)s.pos.y].tile.GetComponent<SpriteRenderer> ().color = pathColor;
		}
	}

	public void Move(List<Step> path, int step) {
		Step s = path[step];
		if (s.pos == end || s.pos == start)
			return;
		tileMap [(int)s.pos.x + width * (int)s.pos.y].tile.GetComponent<SpriteRenderer> ().color = pathColor;
		Debug.Log (s.pos);
	}

	public void ClearPaths() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (tileMap [y * width + x].type == 5) {
					tileMap [y * width + x].type = 0;
					tileMap [y * width + x].tile.GetComponent<SpriteRenderer> ().color = voidColor;
				}
			}
		}
	}

}



[System.Serializable]
public class TileInfo : System.Object {
	public int type, x, y;
	public GameObject tile;

	public TileInfo(GameObject _tile, int _type, int _x, int _y) {
		type = _type;
		tile = _tile;
		x = _x;
		y = _y;
	}

}
	
public class Step {

	public int count;

	public Vector2 pos;
	public Vector2 fromWhere;


	public Step(Vector2 _pos, Vector2 _fromWhere, int _count){
		pos = _pos;
		fromWhere = _fromWhere;
		if (_count == null)
			count = 0;
		else
			count = _count;
	}
}