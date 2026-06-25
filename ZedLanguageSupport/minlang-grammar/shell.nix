with import <nixpkgs> { };

mkShell {
  packages = [
    llvmPackages.clang
    llvmPackages.libclang
    gcc
    python3
    graphviz
    gdb
  ];

  LIBCLANG_PATH = "${llvmPackages.libclang.lib}/lib";
}
