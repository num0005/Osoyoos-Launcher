## Feature | Generate Named and Linked Shaders

This guide is intended to introduce a new feature added to **Osoyoos**. This feature allows for the automatic generation of shader tags for custom maps created using **blender** and the **Halo Asset Blender Development Toolset**.  

This guide will cover a basic custom map workflow compatible with this new feature. We'll import a custom map into **blender**, export the map and textures, and finally import the map with **Osoyoos** while automatically generating the shader tags with correctly linked bitmaps.

## Table of Contents
- [Feature | Generate Named and Linked Shaders](#feature--generate-named-and-linked-shaders)
- [Table of Contents](#table-of-contents)
- [Prerequisites](#prerequisites)
	- [General Knowledge](#general-knowledge)
	- [Applications and Add-ons](#applications-and-add-ons)
- [Get the Map](#get-the-map)
- [Map Preparation](#map-preparation)
	- [Edit `shader_collections.txt`](#edit-shader_collectionstxt)
	- [Open and Inspect the Map](#open-and-inspect-the-map)
	- [Basic Map Setup](#basic-map-setup)
	- [Hierarchy / Parenting](#hierarchy--parenting)
	- [Batch Rename Materials](#batch-rename-materials)
	- [`Save` your blender scene.](#save-your-blender-scene)
- [**blender** Export](#blender-export)
	- [`Export` Textures](#export-textures)
	- [`Export` the `.ASS` File](#export-the-ass-file)
- [Osoyoos](#osoyoos)
	- [Import Bitmaps](#import-bitmaps)
	- [Import \& Light Level](#import--light-level)
- [Finishing Up](#finishing-up)

## Prerequisites

### General Knowledge
- Basic understanding of 3D modeling and texturing in the context of **blender** and **Halo 3**.
- Familiarity with **Halo Editing Kit** tools and **Sapien**.
- Understanding of **Osoyoos** and the **Halo Map Creation** process.

### Applications and Add-ons
- **[Blender 4.3](https://www.blender.org/download/)**: 3D modeling and texturing software.
- **[Foundry](https://c20.reclaimers.net/general/community-tools/foundry/)**: A **Blender** add-on for streamlining import and export of Halo assets.
- **[Halo Asset Blender Development Toolset](https://github.com/General-101/Halo-Asset-Blender-Development-Toolset/releases/233)**: A **Blender** add-on for working with Halo assets.
- **[Osoyoos](https://github.com/num0005/Osoyoos-Launcher/releases/tag/1.0.80%2B0b0b0be3e6)**: A graphical frontend for automating interactions with Halo Editing Kit tools.
- **[Crafty](https://nemstools.github.io/pages/Crafty.html)**: Tool for extracting maps from Source and GoldSrc games. *(Repository: [Crafty GitHub](https://github.com/nemstools/nemstools.github.io/tree/master))*

## Get the Map
- Download a map containing existing texture data from your desired source.  
(Example: We used "[*Temple of Oss*](https://gamebanana.com/mods/573388)", a CounterStrike 1.6 map from *Game Banana*.)
- Use **Crafty** (if using a Source or GoldSrc map) to extract the map to **`OBJ`** format.  

## Map Preparation  

### Edit `shader_collections.txt`  
- The `shader_collections.txt` file **must** include a definition for your map's prefix and location.
- The file should be located at `...\H3EK\tags\levels\shader_collections.txt`.
- In this example, we added the following line to the end of the file:  
`tmpl		levels\custom\temple`  
- This line defines the prefix `tmpl` for our custom `temple` level located at `levels\custom\temple`.  

### Open and Inspect the Map
- Import the **`OBJ`** file into **blender**.
- Inspect the map and ensure it has the materials you expect to see.
- The materials should be visible in **blender** when the scene is opened.  

### Basic Map Setup  
- **`Select`** the **map object** in the hierarchy.
- Enter **`Edit Mode`** and **`Select All`** (**`A` Key**).
- Apply the following operations to the map object:
  - **`Triangulate`**
  - **`Merge by Distance`**
  - **`Mark Sharp`**
- Exit **`Edit Mode`**.  

### Hierarchy / Parenting  

- Add a **UV Sphere** to the scene.  
(**`Add`** → **`Mesh`** → **`UV Sphere`**)
- Select the **UV Sphere** and rename it **`b_levelroot`**.
- Parent the **map object** to the **`b_levelroot`** object.  
  - **`Select`** the **map object** in the hierarchy.
  - **`CTRL-Select`** the **`b_levelroot`** object in the hierarchy.
  - Due to **blender**'s context-sensitive approach to actions, **ensure your cursor is within the _`3D View`_** before using the keyboard shortcut in the next step.
  - Press **`Ctrl-P (Set Parent To)`** → **`Object`**  

### Batch Rename Materials  

- **`Select`** your **map object** in the hierarchy.
- **`Edit`** → **`Batch Rename`**.  
► The **`Batch Rename`** string format: ```<prefix> <level name><_><material>```  
► **e.g.**, for our custom **`temple`** level: **`tmpl temple_material`**  
- **`Batch Rename`** Settings:
  - **Selected**
  - **Materials**
  - *Type*: **Find/Replace**
  - *Find*: **`material`**
  - *Replace*: **`tmpl temple_material`**
- Click **`OK`** to apply the renaming.  

### `Save` your blender scene.  

> The steps we've taken have been to ensure that all file names are consistent and valid for this process. If performed correctly, this allows for the automatic generation of shader tags in **Osoyoos** when executing the **`Import/Light Level`** command.

## **blender** Export

### `Export` Textures  

- **`Expand`** the *`Sidebar`* (or *`N-Panel`*) using the small arrow in the top right corner of the 3D view, or by pressing the **`N` Key**.
- Open the **`Halo Tools`** tab, which should be present if you have the ***Halo Asset Blender Development Toolset*** installed.
- Set your **H3EK Directory** to the root of your **`H3EK`** folder.
- Set your **Data Directory** to point to the desired texture export directory.  
In this case, it should be something like:  
**`...\H3EK\data\levels\custom\temple\bitmaps`**
- Click the **`Export Textures`** button to export the textures to the specified directory.


### `Export` the `.ASS` File  
- **`File`→`Export`→`Halo Amalgam Scene Specification (.ass)`**
- Configure the **Export Options**
  - **Export Location**: **`...\H3EK\data\levels\custom\temple\`*****`structure\temple.ass`***
  - **Game Title**: **`Halo 3`**
  - **Use Split Edges**: **`False (Unchecked)`**
- Click the **`Export ASS`** button to export the file.  

## Osoyoos

### Import Bitmaps  
- Navigate to the *`Import Bitmaps`* tab.
- **`Browse`**, and select the directory containing our `.tif` textures.
- In this case: `...\H3EK\data\levels\custom\temple\bitmaps`
- Click **`Import Bitmaps`**, this will generate bitmaps in the required location.  

### Import & Light Level  

- Navigate to the *`Import & Light Level`* tab.
- Click **`Browse`** to select the level file to import.
- Select the `temple.ass` file located at  
`...\H3EK\data\levels\custom\temple\structure\temple.ass`
- Review the *`Import Settings`* section.
- Ensure the **`Generate Named Shaders`** checkbox is **`CHECKED`**.
- Click **`Import/Light Level`** to complete the import process.  

## Finishing Up

After completing these steps, **Osoyoos** should have successfully imported the level:
- Shader tags will be automatically generated, correctly referencing the bitmap tags.
- No missing material errors should appear.
- The level should load in **Sapien** with all materials applied and visible!

I hope this feature proves useful in your map-making endeavors!

— **BIRD COMMAND**