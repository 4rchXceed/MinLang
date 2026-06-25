./build.sh
git add . && git commit -m "_"
rm ../extension.toml
cp ../extension.toml.sample ../extension.toml
echo rev = \"$(git rev-parse HEAD)\" >> ../extension.toml
