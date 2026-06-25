# Edit variables if needed
if ! [ -x "$(command -v git)" ]; then
  echo 'Error: git is not installed. Required!' >&2
  exit 1
fi
if ! [ -f "$HOME/.local/share/zed/extensions/index.json" ]; then
  echo 'Error: Zed extensions index not found. Required! (Is zed installed?)' >&2
  exit 1
fi
if [ -d "$HOME/.local/share/zed/extensions/installed/minlang-language-server" ]; then
  echo 'Error: MinLang language server is already installed. Please uninstall first (Zed -> Extensions -> MinLang Language Server -> Uninstall).' >&2
  exit 1
fi
cp -r ./ "$HOME/.local/share/zed/extensions/installed/minlang-language-server"
cd "$HOME/.local/share/zed/extensions/installed/minlang-language-server"
# First: create git repository
cd minlang-grammar
git init
git add .
git commit -m 'Auto-Commit'
# Find the rev
REV="$(git rev-parse HEAD)"
REPO="file:///$HOME/.local/share/zed/extensions/installed/minlang-language-server/minlang-grammar"
cd ..
# Git clone
git clone $REPO ./grammars/minlang
# Declare the script
SCRIPT="
import json
import tomllib
with open('$HOME/.local/share/zed/extensions/index.json', 'r', encoding='utf-8') as f_src:
    content = json.load(f_src)
with open('./extension.toml', 'rb') as f_src:
    manifest = tomllib.load(f_src)
with open('./extension.toml', 'r', encoding='utf-8') as f_src:
    text_manifest = f_src.read()
repo = '$REPO'
content['extensions']['minlang-language-server'] = {
    'manifest': {
        'id': 'minlang-language-server',
        'name': 'MinLang Language Server',
        'version': '1.0.0',
        'schema_version': 1,
        'description': 'Language Server for https://4rchxceed.github.io/MinLang-docs',
        'repository': 'https://github.com/4rchXceed/MinLang',
        'authors': [
            'Lyam Zambaz <lyam.zambaz@pm.me>'
        ],
        'lib': {
            'kind': None,
            'version': None
        },
        'themes': [],
        'icon_themes': [],
        'languages': [
            'languages/minlang'
        ],
        'grammars': {
            'minlang': {
                'repository': repo,
                'rev': '$REV',
                'path': None
            }
        },
        'language_servers': {},
        'context_servers': {},
        'agent_servers': {},
        'slash_commands': {},
        'snippets': [
            './snippets/minlang.json'
        ],
        'capabilities': []
    },
    'dev': True
}
with open('$HOME/.local/share/zed/extensions/index.json', 'w', encoding='utf-8') as f_dst:
    json.dump(content, f_dst, indent=2)
with open('./extension.toml', 'w', encoding='utf-8') as f_dst:
    f_dst.write(text_manifest.replace(manifest['grammars']['minlang']['repository'], repo).replace(manifest['grammars']['minlang']['rev'], '$REV'))
"
echo "Creating temporary script"
echo "$SCRIPT" > /tmp/minlang_install.py
echo "Creating extensions' index.json backup (at $HOME/.local/share/zed/extensions/index.json.bak)"
if [ -x $(command -v nix-shell) ]; then
    echo "nix-shell found, installing..."
    cp extension.toml.sample extension.toml # Use a clean file
    nix-shell -p python3 --run "python3 /tmp/minlang_install.py"
    if [ $? -eq 0 ]; then
        echo "Installation successful!"
        exit 0
    else
        echo "Installation failed. Reverting to backup..."
        cd -
        rm -rf "$HOME/.local/share/zed/extensions/installed/minlang-language-server"
        cp -r "$HOME/.local/share/zed/extensions/index.json.bak" "$HOME/.local/share/zed/extensions/index.json"
        exit 1
    fi
fi
if ! [ -x "$(command -v python3)" ]; then
  echo 'Error: python3 is not installed.' >&2
  exit 1
fi
echo "Installing normally..."
cp extension.toml.sample extension.toml # Use a clean file
python3 /tmp/minlang_install.py
if [ $? -eq 0 ]; then
    echo "Installation successful!"
    exit 0
else
    echo "Installation failed. Reverting to backup..."
    cd -
    rm -rf "$HOME/.local/share/zed/extensions/installed/minlang-language-server"
    cp -r "$HOME/.local/share/zed/extensions/index.json.bak" "$HOME/.local/share/zed/extensions/index.json"
    exit 1
fi
