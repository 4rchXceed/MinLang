/**
 * @file Language Server for https://4rchxceed.github.io/MinLang-docs
 * @author Lyam Zambaz <lyam.zambaz@pm.me>
 * @license MIT
 */

/// <reference types="tree-sitter-cli/dsl" />
// @ts-check

export default grammar({
  name: "minlang",

  externals: ($) => [$.start, $.end],

  rules: {
    source_file: ($) => repeat($._definition),
    _definition: ($) =>
      seq(
        choice(
          $.function_definition_normal,
          $.function_definition_ascode,
          $.global_var_definition,
          $.import_statement,
        ),
        optional($.comment),
      ),
    // "Main" parts
    function_definition_normal: ($) =>
      seq(
        optional($.loop_keyword),
        $.fun_keyword,
        /[ \t]+/,
        $.start,
        $.parameter_list_normal,
        ":",
        optional($.comment),
        "\n",
        $.block,
      ),
    function_definition_ascode: ($) =>
      seq(
        $.ascode_keyword,
        $.fun_keyword,
        /[ \t]+/,
        $.start,
        $.parameter_list_global,
        ":",
        choice($.comment, "\n"),
        $.block,
      ),
    global_var_definition: ($) =>
      seq($.const_keyword, $.typedec, $.globalvar_name_decl, "=", $.val),
    import_statement: ($) => seq($.import_keyword, $.import_path, "\n"),
    // More specific parts
    parameter_list_normal: ($) =>
      seq($.bracket_open, optional($.arglist_normal), $.bracket_close),
    parameter_list_global: ($) =>
      seq($.bracket_open, optional($.arglist_global), $.bracket_close),
    arglist_normal: ($) =>
      seq(
        repeat(seq(optional($.argument_delim), $.typedec, $.localvar_name)),
        $.argument_delim,
      ),
    arglist_global: ($) =>
      seq(
        repeat(seq(optional($.argument_delim), $.typedec, $.globalvar_name)),
        $.argument_delim,
      ),
    call_arglist: ($) => repeat1(seq(optional($.argument_delim), $.val_free)),
    // Actual code
    block: ($) => seq(repeat($.statement), $.end),
    statement: ($) => seq($.line_code, optional($.comment), "\n"),
    line_code: ($) =>
      choice($.function_call, $.if, $.for, $.var_set, $.var_create, $.comment),
    // Types of lines
    function_call: ($) =>
      seq(
        $.function_name,
        $.bracket_open,
        $.bracket_close,
        $.lt,
        optional($.call_arglist),
        $.gt,
      ),
    if: ($) =>
      seq(
        $.if_keyword,
        $.bracket_open,
        $.local_var,
        $.comparator,
        choice($.val, $.global_var),
        $.bracket_close,
        ":",
        $.function_call,
      ),
    for: ($) =>
      seq(
        $.for_keyword,
        $.bracket_open,
        $.int,
        $.argument_delim,
        $.int,
        $.argument_delim,
        $.int,
        $.bracket_close,
        "->",
        $.localvar_name,
        ":",
        $.function_call,
      ),
    var_set: ($) =>
      seq(
        "#::",
        choice(
          $.localvar_name,
          seq(
            $.braces_open,
            choice($.global_var, $.special_string),
            $.braces_close,
          ),
        ),
        "::",
        $.operator,
        $.bracket_open,
        $.bracket_close,
        $.lt,
        $.int,
        $.gt,
      ),
    var_create: ($) =>
      seq(
        $.int_type,
        choice(
          $.localvar_name,
          seq(
            $.braces_open,
            choice($.global_var, $.special_string),
            $.braces_close,
          ),
        ),
        "=",
        choice($.int, $.global_var),
      ),
    operator: ($) => choice("Set", "Add", "Sub"),
    comment: ($) => /\/\/[^\n]*/,
    // Other
    typedec: ($) => choice("Int", "Float", "String", "Bool", "Void"),
    import_path: ($) => /[^\n]+\/?([^\/]*)\.uchc|ucl/,
    comparator: ($) =>
      choice("=", "<", ">", "<=", ">=", "!=", "!<", "!>", "!<=", "!>="),
    // Values
    val_free: ($) =>
      choice(
        $.val,
        $.local_var,
        $.global_var,
        // <%Print()<...>%>
        seq(
          $.function_ptr_keyword_open,
          $.function_call,
          $.function_ptr_keyword_close,
        ),
        $.special_string,
      ), // Can reference other vals
    local_var: ($) =>
      seq(
        "#::",
        choice(
          $.localvar_name,
          seq(
            $.braces_open,
            choice($.global_var, $.special_string),
            $.braces_close,
          ),
        ),
      ),
    global_var: ($) => seq("$::", choice($.self_keyword, $.globalvar_name)),
    val: ($) => choice($.int, $.float, $.string, $.bool, $.void), // Builtin types ONLY
    special_string: ($) =>
      seq(
        $.concat_str_keyword,
        '"',
        repeat1(
          choice(
            seq(
              optional($.string_inside),
              optional(
                seq(
                  $.special_string_open,
                  $.global_var,
                  $.special_string_close,
                ),
              ),
              $.string_inside,
            ),
            seq(
              $.string_inside,
              optional(
                seq(
                  $.special_string_open,
                  $.global_var,
                  $.special_string_close,
                ),
              ),
              optional($.string_inside),
            ),
          ),
        ),
        '"',
      ),
    string_inside: ($) => /([^"\n$]|\$[^{"\n])+/,
    int: ($) => /(-?[0-9]+)/,
    float: ($) => /(-?[0-9]+\\.[0-9]+)/,
    string: ($) => /\"([^\"\n]*)\"|\'([^\'\n]*)\'/,
    bool: ($) => /(true|false)/,
    void: ($) => "null",
    // Keywords
    self_keyword: ($) => "self",
    localvar_name: ($) => /[A-Za-z0-9_]+/,
    function_name: ($) => /[A-Za-z0-9_]+/,
    globalvar_name_decl: ($) => /\$[A-Za-z0-9_]+/,
    globalvar_name: ($) => /[A-Za-z0-9_]+/,
    bracket_open: ($) => "(",
    bracket_close: ($) => ")",
    braces_open: ($) => "{",
    braces_close: ($) => "}",
    lt: ($) => "<",
    gt: ($) => ">",
    fun_keyword: ($) => "fun",
    const_keyword: ($) => "const",
    import_keyword: ($) => "%import",
    ascode_keyword: ($) => "ascode",
    argument_delim: ($) => ";",
    if_keyword: ($) => "If",
    for_keyword: ($) => "For",
    function_ptr_keyword_open: ($) => "<%",
    function_ptr_keyword_close: ($) => "%>",
    concat_str_keyword: ($) => "&",
    special_string_open: ($) => "${",
    special_string_close: ($) => "}",
    loop_keyword: ($) => "loop",
    int_type: ($) => "Int",
  },
});
