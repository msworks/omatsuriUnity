using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Banner : MonoBehaviour {

    public List<Sprite> sprites;

    public float span = 2.0f;

	void Start () {
        StartCoroutine(anim());
	}

    private IEnumerator anim()
    {
        var image = GetComponent<Image>();

        foreach(var sprite in new CycleSequence<Sprite>(sprites))
        {
            yield return new WaitForSeconds(span);
            image.sprite = sprite;
        }
    }

}

public class CycleSequence<T> : IEnumerable<T>
{
    protected List<T> list;

    public CycleSequence(List<T> reel) { list = reel; }

    public IEnumerator<T> GetEnumerator()
    {
        while (true)
        {
            foreach (T rl in list)
            {
                yield return rl;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
