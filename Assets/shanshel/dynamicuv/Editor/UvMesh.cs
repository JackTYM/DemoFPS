using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace shanshel.duv
{
    [SerializeField]
    public  class UvMesh
    {
        public string name;
        public List<UvColor> colors = new List<UvColor>();
        public List<UvPointGroup> groups = new List<UvPointGroup>();
    }

}
