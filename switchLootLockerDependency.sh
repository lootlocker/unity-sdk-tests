#!/bin/bash
echo "Updating ${1}/Packages/manifest.json to use lootlocker sdk at relative path (from manifest) ${2}"
echo "installing jq"
sudo apt-get install -y jq
jq ".\"dependencies\".\"com.lootlocker.lootlockersdk\"=\"${2}\"" "${1}/Packages/manifest.json" > "${1}/Packages/tempmanifest.json"
rm "${1}/Packages/manifest.json"
mv "${1}/Packages/tempmanifest.json" "${1}/Packages/manifest.json"