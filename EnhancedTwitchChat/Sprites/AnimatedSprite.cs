﻿using EnhancedTwitchChat.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedTwitchChat.Sprites
{
    class AnimatedSprite : MonoBehaviour
    {
        private Image _image;
        private List<AnimationData> _spriteList;

        void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(this);

        }

        public void Init(Image image, List<AnimationData> sprites)
        {
            _spriteList = sprites;
            _image = image;
            if (_spriteList.Count > 0)
                _image.sprite = _spriteList[0].sprite;
        }

        void Update()
        {
            if (_spriteList == null || _spriteList.Count <= 1 || AnimationController.Instance == null) return;
            _image.sprite = AnimationController.Instance.Get(_spriteList);
        }

        void OnDestroy()
        {
            _spriteList = null;
        }
    };
}
