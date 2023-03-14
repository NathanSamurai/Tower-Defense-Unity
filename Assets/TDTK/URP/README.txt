URP are not supported by default for two reasons. Materials and Cameras.


The materials in TBTK use standard unity shaders which are not compatible with URP. To fix the shader, you can follow these steps:

1. Use the drop down menu - Edit > Render Pipeline > Universal Render Pipeline > Upgrade Project Materials to Universal Materials. This will cover most material in the project.

2. Look through the subfolders of \Assets\TBTK\CustomMesh\Modular_SciFi_Asset_Set for Materials folders containing unconverted (purple) materials. Switch the shader of all these unconverted materials to "Shader Graphs/BlendColorsOverlayTextureURP".

3. For remaining objects that still doesnt render properly (have a flat purple surface), you will need to manually update their material to the standard URP materials which can be found in \Packages\Universal RP\Runtime\Materials



Next, TBTK uses multiple cameras to render different things. Unfortunately URP doesn't support multiple cameras by default. Instead, a camera stack order has to be set up manually for this to work. Following are the steps to do that:

1. Find all the cameras in the scene. This can be done by typing "t:Camera" into the Hierarchy's search bar.

2. For every camera except the Main Camera, set its Render Type property to Overlay.

3. On the Main Camera, add all other cameras to its Stack List property. The stack order should be as follow:
        - Main Camera
        - Camera  (the camera under UI_TBTK) that used to render UI



The last steps for material fix and the camera fix needs to be repeat everytime you create a new scene via the drop down menu. If you want to fix that once and for all, just apply the changes to the 'TBTK_HexGrid' and 'TBTK_SqGrid' prefab in \Asset\TBTK\Resources\NewScenePrefab\



**Huge thanks to Mason Wheeler of Stormhunter Studios for the informing me of the material fix and providing the BlendColorsOverlayTextureURP shader