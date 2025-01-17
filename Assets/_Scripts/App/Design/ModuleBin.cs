using GLTFast.Schema;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ModuleBin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void OnTriggerEnter(Collider other)
    {
        Debug.Log("Module destroyed");
        if (other.GetComponent<Module>() != null)
        {
            other.GetComponent<Module>().animator.enabled = true;
            other.GetComponent<Module>().animator.Play(other.GetComponent<Module>().clipName);

            await Task.Delay(1000);

            if (other!= null) { 
            other.gameObject.GetComponent<Module>().DestroyModuleServerRPC();
            }
        }
        

        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(collision.gameObject);
    }
}
