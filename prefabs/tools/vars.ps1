$env:PREFABS_REPO = "DecentM/world-prefabs-source"
$env:PREFABS_FOLDER = "prefabs"
# PREFABS_ARTIFACT_NAME - Set from CI as env variable
# PREFABS_RUN_ID - Set from CI as env variable

$env:PREFABS_VERSION = "v1.0.3"
$env:PREFABS_FILENAME = "DecentM.Components.zip"
$env:PREFABS_URL = "https://github.com/$env:PREFABS_REPO/releases/download/$env:PREFABS_VERSION/$env:PREFABS_FILENAME"
