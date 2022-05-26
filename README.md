# Reference Package for Unity

This is a general purpose package that gives ability to work with asset references in your project in async paradigm.

## Installation
Simply add as git package in unity.

## Usage

* Define serialized fields of necessery type instead of using direct ones or AssetReference.
``
* Use async api to access assets.
* Conrol the way asset is referenced in directly in inspector.

## Package support

### [UniTask](https://github.com/Cysharp/UniTask)

This package supports UniTask and it will be used if added to project, no need for defines or code changes.

### [Addressables](https://docs.unity3d.com/Manual/com.unity.addressables.html)

This package is mostly designed to use addressables as underlaying resource manager and it will be used as default if added to project.
But you also free to adapt your own res manager by implementing **IAssetService** interface and registering it by setting **AssetService.Current** before trying to use any Reference.