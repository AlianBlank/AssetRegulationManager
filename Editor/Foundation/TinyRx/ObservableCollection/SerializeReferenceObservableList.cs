﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetRegulationManager.Editor.Foundation.TinyRx.ObservableCollection
{
    [Serializable]
    public abstract class SerializeReferenceObservableList<T> : ObservableListBase<T>
    {
        [SerializeReference] private List<T> _internalList = new List<T>();

        protected override List<T> InternalList => _internalList;
    }
}
