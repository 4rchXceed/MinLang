#include "tree_sitter/parser.h"
#include "tree_sitter/alloc.h"
#include "tree_sitter/array.h"
#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>


enum TokenType {
  START,
  END
};

typedef struct {
  char *fun_name;
} Scanner;

void *tree_sitter_minlang_external_scanner_create() {
  Scanner *s = malloc(1);
  s->fun_name = NULL;
  return s;
}

void tree_sitter_minlang_external_scanner_destroy(void *payload) {
  Scanner *s = payload;
  if (s->fun_name) {
    free(s->fun_name);
  }
  free(s);
}

unsigned tree_sitter_minlang_external_scanner_serialize(
  void *payload,
  char *buffer
) {
  Scanner *s = payload;

  size_t len = strlen(s->fun_name);

  memcpy(buffer, s->fun_name, len);

  return len;
}

void tree_sitter_minlang_external_scanner_deserialize(
  void *payload,
  const char *buffer,
  unsigned length
) {
  Scanner *s = payload;
  free(s->fun_name);

  if (length == 0) {
      s->fun_name = malloc(1);
      s->fun_name = NULL;
      return;
  }


  s->fun_name = malloc(length + 1);
  memcpy(s->fun_name, buffer, length);
  s->fun_name[length] = '\0';
}

static bool check_char(char ch) {
    if(!(ch >= 'a' && ch <= 'z') && !(ch >= 'A' && ch <= 'Z') && !(ch >= '0' && ch <= '9')) {
        return false;
    }
    return true;
}

bool tree_sitter_minlang_external_scanner_scan(
    void *payload,
    TSLexer *lexer,
    const bool *valid_symbols
) {
    Scanner *scanner_data = payload;
    bool status = true;

    if (valid_symbols[START]) {
        // https://stackoverflow.com/questions/29439283/how-to-get-char-with-unknown-length-in-c
        scanner_data->fun_name = malloc(1);
        int i = 0;
        while (check_char(lexer->lookahead)) {
            scanner_data->fun_name[i++] = lexer->lookahead;
            scanner_data->fun_name = realloc(scanner_data->fun_name, i+1);
            lexer->advance(lexer, false);
        }
        scanner_data->fun_name[i] = '\0';
        lexer->result_symbol = START;
    } else if (valid_symbols[END]) {
        int i = 0;
        bool ok = true;
        int max = strlen(scanner_data->fun_name);
        while (ok && i < max) {
            if (scanner_data->fun_name[i] == lexer->lookahead) {
                lexer->advance(lexer, false);
                i++;
            } else {
                ok = false;
            }
        }
        status = ok;
        lexer->result_symbol = END;
    }

    return status;
}
