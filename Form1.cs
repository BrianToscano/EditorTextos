using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace EditorTextos
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            compilarSoluciónToolStripMenuItem.Enabled = false;
            //inicializa la opcion de compilar como inhabilitada.
        }
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog VentanaAbrir = new OpenFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (VentanaAbrir.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaAbrir.FileName;
                using (StreamReader Leer = new StreamReader(archivo))
                {
                    richTextBox1.Text = Leer.ReadToEnd();
                }

            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
            compilarSoluciónToolStripMenuItem.Enabled = true;
            //habilita la opcion compilar cuando se carga un archivo.
        }
        private void Guardar()
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (archivo != null)
            {
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            else
            {
                if (VentanaGuardar.ShowDialog() == DialogResult.OK)
                {
                    archivo = VentanaGuardar.FileName;
                    using (StreamWriter Escribir = new StreamWriter(archivo))
                    {
                        Escribir.Write(richTextBox1.Text);
                    }
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
        }
        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Guardar();

        }
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            archivo = null;
        }
        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog
            {
                Filter = "Texto|*.c"
            };
            if (VentanaGuardar.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaGuardar.FileName;
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
        }
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            compilarSoluciónToolStripMenuItem.Enabled = true;
            //habilita la opcion compilar cuando se realiza un cambio en el texto.
        }

        //////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////funciones del analisis lexico/////////////////////////
        ///
        private char Tipo_caracter(int caracter)
        {
            if (caracter >= 65 && caracter <= 90 || caracter >= 97 && caracter <= 122) { return 'l'; } //letra 
            else
            {
                if (caracter >= 48 && caracter <= 57) { return 'd'; } //digito 
                else
                {
                    switch (caracter)
                    {
                        case 10: return 'n'; //salto de linea
                        case 34: return '"';//inicio de cadena
                        case 39: return 'c';//inicio de caracter
                        case 47: return '/';//inicio de comentario de linea o de bloque
                        case 32: return 'e';//espacio
                        default: return 's';//simbolo
                    }

                }
            }

        }
        private void Simbolo()
        {
            if (i_caracter == 33 ||
                i_caracter >= 35 && i_caracter <= 38 ||
                i_caracter >= 40 && i_caracter <= 45 ||
                i_caracter >= 58 && i_caracter <= 62 ||
                i_caracter == 91 ||
                i_caracter == 93 ||
                i_caracter == 94 ||
                i_caracter == 123 ||
                i_caracter == 124 ||
                i_caracter == 125
                ) { elemento = elemento + (char)i_caracter + "\n"; } //simbolos validos 
            else { Error(i_caracter); }
        }
        private void Cadena()
        {
            do
            {
                i_caracter = Leer.Read();
                if (i_caracter == 10) Numero_linea++;
            } while (i_caracter != 34 && i_caracter != -1);
            if (i_caracter == -1) Error(34);
        }
        private void Caracter()
        {
            i_caracter = Leer.Read();
            //programar para los casos donde el caracter se imprime  '\n','\r','\t' etc.
            i_caracter = Leer.Read();
            if (i_caracter != 39) Error(39);
        }
        private void Error(int i_caracter)
        {
            Rtbx_salida.AppendText("Error léxico " + (char)i_caracter + ", línea " + Numero_linea + "\n");
            N_error++;
        }
        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h') { elemento = "Libreria\n"; i_caracter = Leer.Read(); }
            else { Error(i_caracter); }
        }
        private bool Palabra_Reservada()
        {
            if (P_Reservadas.IndexOf(elemento) >= 0) return true;
            return false;
        }
        private void Identificador()
        {
            do
            {
                elemento += (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');
            if (Palabra_Reservada()) elemento += "\n";
            else
            {
                switch (i_caracter)
                {
                    case '.': Archivo_Libreria(); break;
                    case '(': elemento = "funcion\n"; break;
                    default: elemento = "identificador\n"; break;
                }
            }
        }
        private bool Comentario()
        {
            i_caracter = Leer.Read();
            switch (i_caracter)
            {
                case 47:
                    do
                    {
                        i_caracter = Leer.Read();
                    } while (i_caracter != 10);
                    return true;
                case 42:
                    do
                    {
                        do
                        {
                            i_caracter = Leer.Read();
                            if (i_caracter == 10) { Numero_linea++; }
                        } while (i_caracter != 42 && i_caracter != -1);
                        i_caracter = Leer.Read();
                    } while (i_caracter != 47 && i_caracter != -1);
                    if (i_caracter == -1) { Error(i_caracter); }
                    i_caracter = Leer.Read();
                    return true;
                default: return false;
            }
        }
        private void Numero_Real()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            elemento = "numero_real\n";
        }
        private void Numero()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            if ((char)i_caracter == '.') { Numero_Real(); }
            else
            {
                elemento = "numero_entero\n";
            }
        }
        ///////////////////Inicio del analisis léxico////////////////////////////
        /////////////////////////////////////////////////////////////////////////
        private void compilarSoluciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rtbx_salida.Text = "Analizando...\n";
            Guardar(); elemento = "";
            N_error = 0; Numero_linea = 1;
            archivoback = archivo.Remove(archivo.Length - 1) + "back";
            Escribir = new StreamWriter(archivoback);
            Leer = new StreamReader(archivo);
            i_caracter = Leer.Read();
            do
            {
                elemento = "";
                switch (Tipo_caracter(i_caracter))
                {
                    case 'l': Identificador(); Escribir.Write(elemento); break;
                    case 'd': Numero(); Escribir.Write(elemento); break;
                    case 's': Simbolo(); Escribir.Write(elemento); i_caracter = Leer.Read(); break;
                    case '"': Cadena(); Escribir.Write("cadena\n"); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.Write("caracter\n"); i_caracter = Leer.Read(); break;
                    case '/': if (Comentario()) { Escribir.Write("comentario\n"); } else { Escribir.Write("/\n"); } break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; Escribir.Write("LF\n"); break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                };

            } while (i_caracter != -1);
            Escribir.Write("Fin");
            Escribir.Close();
            Leer.Close();
            if (N_error == 0) { Rtbx_salida.AppendText("Errores Lexicos: " + N_error); A_Sintactico(); }
            else { Rtbx_salida.AppendText("Errores: " + N_error); }
        }


        //////////////////////////////////////////////////////////////////////////
        ////////////////////Funciones del análisis sintáctico///////////////////////////////////
        private void Errors(string e, string s)
        {
            Rtbx_salida.AppendText("Linea: " + Numero_linea + ". Error de sintaxis " + e + ", se esperaba " + s + "\n");
            token = ""; N_error++;
        }
        //----------------------------------------------------------------------------
        private void Include()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "<":
                    token = Leer.ReadLine();
                    if (token == "Libreria")
                    {
                        token = Leer.ReadLine();
                        if (token == ">")
                        {
                            token = Leer.ReadLine();
                        }
                        else { ErrorS(token, ">"); N_error++; }
                    }
                    else { ErrorS(token, "nombre de archivo libreria"); N_error++; }
                    break;
                case "cadena": token = Leer.ReadLine(); break;
                //case "identificador": token = Leer.ReadLine(); break;
                default: ErrorS(token, "inclusión valida "); N_error++; break;
            }
        }
        //--------------------------------------------------------------------------
        private void Directriz()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "include": Include(); break;
                case "define"://estructura para directriz #define 
                    break;
                case "if":    //estructura para directriz #if
                    break;
                case "error":    //estructura para directriz #error
                    break;
                // misma forma para las restantes directivas de procesador,
                default: ErrorS(token, "directriz de procesador"); break; ;
            }
        }
        //---------------------------------------------------------------------------
        private int Constante()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "numero_real": return 1;
                case "numero_entero": return 1;
                case "caracter": return 1;
                case "identificador": return 1;
                default: return 0;
            }
        }
        //-----------------------------------------------------------------------------
        private void Bloque_Inicializacion()
        {
            do
            {
                token = Leer.ReadLine();
                if (token == "{")
                {
                    do
                    {
                        if (Constante() == 1) { token = "elemento"; }
                        switch (token)
                        {
                            case "elemento": token = Leer.ReadLine(); break;
                            case "{":
                                do
                                {
                                    if (Constante() == 0) { ErrorS(token, " inicializacion valida de arreglo."); }
                                    else { token = Leer.ReadLine(); }
                                } while (token == ",");
                                if (token == "}") { token = Leer.ReadLine(); }
                                else { ErrorS(token, "}"); }
                                break;
                        }
                    } while (token == ",");
                    if (token == "}") { token = Leer.ReadLine(); }
                    else { ErrorS(token, "}"); }
                }
                else { ErrorS(token, "{"); }
            } while (token == ",");
        }
        //-------------------------------------------------------------------------------
        private void D_Arreglos()
        {
            while (token == "[")
            {
                token = Leer.ReadLine();
                if (token == "identificador" || token == "numero_entero")
                {
                    token = Leer.ReadLine();
                    if (token == "]") { token = Leer.ReadLine(); }
                    else { ErrorS(token, "]"); }
                }
                else ErrorS(token, "valor de longitud");
            }
            switch (token)
            {
                case ";": token = Leer.ReadLine(); break;
                case "=":
                    token = Leer.ReadLine();
                    if (token == "{")
                    {
                        Bloque_Inicializacion();
                        if (token == "}")
                        {
                            token = Leer.ReadLine();
                            if (token == ";") { token = Leer.ReadLine(); }
                            else { ErrorS(token, ";"); }
                        }
                        else { ErrorS(token, "}"); }
                    }
                    else { ErrorS(token, "{"); }
                    break;
                default: ErrorS(token, "declaracion valida para arreglos."); break;
            }
        }
        //----------------------------------------------------------------------------
        private void Dec_VGlobal() //se ha leido tipo e identificador
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "=":
                    if (Constante() == 1)
                    {
                        token = Leer.ReadLine();
                        if (token == ";") { token = Leer.ReadLine(); }
                        else { ErrorS(token, ";"); }
                    }
                    else { ErrorS(token, "inicializacion global valida"); }
                    break;
                case "[": D_Arreglos(); break;
                case ";": token = Leer.ReadLine(); break;
                default: ErrorS(token, ";"); break;
            }
        }
        //--------------------------------------------------------------------------
        private void Declaracion()
        {
            switch (token)
            {
                case "identificador": Dec_VGlobal(); break;
                case "funcion": Dec_Funcion(); break;
                default: ErrorS(token, "declaracion global valida"); break;
            }
        }
        private void F_Main() 
        {
            token = Leer.ReadLine();
            if (token == "(")
            {
                token = Leer.ReadLine();
                if (token == ")")
                {
                    token = Leer.ReadLine();
                    Bloque_Sentencia();

                }
                else ErrorS(token, ")");
            }
            else ErrorS(token, "(");
        }
        //-------------------------------------------------------------------------
        private int Cabecera()
        {
            token = Leer.ReadLine();
            do
            {
                if (P_Res_Tipo.IndexOf(token) >= 0) { token = "tipo"; }
                switch (token)
                {    //en este caso practico solamente se considera la directiva #include
                    case "#": Directriz(); break;
                    case "tipo":
                        token = Leer.ReadLine();
                        if (token == "main") return 1;
                        else Declaracion();
                        break;
                    case "comentario": token = Leer.ReadLine(); break;
                    case "typedef": //estructura typedef
                        break;
                    case "const": //estrucutura const
                        break;
                    case "extern": //estrucutura extern
                        break;
                    case "": token = Leer.ReadLine(); break;
                    case "LF": Numero_linea++; token = Leer.ReadLine(); break;
                    default: token = Leer.ReadLine(); break;

                }
            } while (token != "Fin" && token != "main");
            return 0;
        }
        ////////////inicio del análisis sintáctico// // // // // //
        private void A_Sintactico()
        {
            Rtbx_salida.AppendText("\nAnalizando sintaxis...\n");
            N_error = 0; Numero_linea = 1;
            Leer = new StreamReader(archivoback);
            if (Cabecera() == 1)
            { F_Main(); }
            else { ErrorS(token, "funcion main()"); }
            Rtbx_salida.AppendText("Errores sintácticos: " + N_error);
            Leer.Close();
        }
        // Declaraciones necesarias


        private void E_Condicion()
        {
            token = Leer.ReadLine();
            // Leer el primer token de la condición
            while (token == "espacio" || token == "salto de linea")
            // Verificar si el token es un paréntesis de apertura
            {
                token = Leer.ReadLine();  // Leer el siguiente token hasta encontrar algo útil
            }
            if (token == "(")
            {
                token = Leer.ReadLine();  // Leer el siguiente token dentro de los paréntesis

                // Aquí puedes analizar más tokens para verificar si son condiciones válidas
                // Por ejemplo, podrías verificar si el token es un identificador o una constante
                if (EsExpresionValida(token))
                {
                    token = Leer.ReadLine();  // Leer el siguiente token que podría ser un operador o una expresión

                    // Analizar operadores o expresiones dentro de la condición
                    while (token != ")" && token != "fin")
                    {
                        if (EsOperador(token))
                        {
                            // Procesar operadores lógicos/comparativos
                        }
                        else if (EsExpresionValida(token))
                        {
                            // Procesar una nueva parte de la condición
                        }
                        else
                        {
                            // Error si el token no es válido
                            ErrorS(token, "Operador o expresión esperada");
                        }

                        token = Leer.ReadLine();  // Leer el siguiente token
                    }

                    // Verificar que la condición esté cerrada con un paréntesis
                    if (token != ")")
                    {
                        ErrorS(token, "Se esperaba ')'");
                    }
                }
                else
                {
                    ErrorS(token, "Condición inválida");
                }
            }
            else
            {
                ErrorS(token, "Se esperaba '('");
            }
        }

        // Funciones auxiliares:
        private bool EsExpresionValida(string token)
        {
            // Aquí puedes comprobar si el token es una expresión válida, como un identificador o una constante
            return token == "a" || token == "b" || token == "true" || token == "false";  // Ejemplo básico
        }

        private bool EsOperador(string token)
        {
            // Aquí puedes comprobar si el token es un operador lógico/comparativo
            return token == "==" || token == ">" || token == "<" || token == "&&" || token == "||";
        }

        private void ErrorS(string token, string mensaje)
        {
            // Función para manejar errores, imprimir el mensaje y posiblemente detener el análisis
            Console.WriteLine($"Error: {mensaje} en el token '{token}'");
        }


        private void Dec_Funcion()
        {
            // Leer el tipo de retorno de la función (puede ser 'int', 'void', etc.)
            token = Leer.ReadLine();
            if (EsTipoRetorno(token)) // Verifica si es un tipo de retorno válido
            {
                string tipoRetorno = token;

                // Leer el nombre de la función (debe ser un identificador)
                token = Leer.ReadLine();
                if (!EsIdentificador(token))
                {
                    ErrorS(token, "Nombre de la función no válido");
                    return;
                }
                string nombreFuncion = token;

                // Leer el paréntesis de apertura '(' para los parámetros
                token = Leer.ReadLine();
                if (token != "(")
                {
                    ErrorS(token, "Se esperaba '(' para iniciar los parámetros de la función");
                    return;
                }

                // Procesar los parámetros de la función (si los hay)
                LeerParametros();

                // Leer el paréntesis de cierre ')'
                token = Leer.ReadLine();
                if (token != ")")
                {
                    ErrorS(token, "Se esperaba ')' para cerrar los parámetros de la función");
                    return;
                }

                // Leer la apertura del bloque de sentencias '{'
                token = Leer.ReadLine();
                if (token != "{")
                {
                    ErrorS(token, "Se esperaba '{' para iniciar el bloque de la función");
                    return;
                }

                // Procesar el bloque de sentencias de la función
                Bloque_Sentencia();

                // Leer la llave de cierre '}'
                token = Leer.ReadLine();
                if (token != "}")
                {
                    ErrorS(token, "Se esperaba '}' para cerrar el bloque de la función");
                }
            }
            else
            {
                ErrorS(token, "Tipo de retorno no válido");
            }
        }

        private void LeerParametros()
        {
            token = Leer.ReadLine();
            while (token != ")")
            {
                // Verificar si el token es un tipo válido para un parámetro (ej. int, char, etc.)
                if (EsTipoRetorno(token))
                {
                    // Leer el tipo del parámetro
                    string tipoParametro = token;

                    // Leer el nombre del parámetro
                    token = Leer.ReadLine();
                    if (!EsIdentificador(token))
                    {
                        ErrorS(token, "Nombre de parámetro no válido");
                        return;
                    }
                    string nombreParametro = token;

                    token = Leer.ReadLine();  // Leer siguiente token
                }
                else
                {
                    ErrorS(token, "Se esperaba tipo de parámetro");
                    break;
                }
            }
        }

        private void Bloque_Sentencia()
        {
            token = Leer.ReadLine();
            if (token == "{")
            {
                token = Leer.ReadLine();

                // Usamos un bucle para procesar múltiples sentencias dentro del bloque
                while (token != "}" && token != "fin")
                {
                    switch (token)
                    {
                        case "{":
                            Bloque_Sentencia();
                            break;
                        case "}":
                            return;  // Salimos del bloque al encontrar '}'
                        default:
                            Sentencias();
                            break;
                    }

                    // Leer el siguiente token para el próximo ciclo
                    token = Leer.ReadLine();
                }

                // Validamos si el bloque no fue cerrado correctamente
                if (token != "}")
                {
                    ErrorS(token, "Se esperaba '}' para cerrar el bloque");
                }
            }
            else
            {
                ErrorS(token, "{");  // Si el bloque no empieza con '{'
            }
        }

        // Función para manejar las sentencias dentro del bloque
        private void Sentencias()
        {
            token = Leer.ReadLine();  // Leer el siguiente token
            Console.WriteLine("Token leído: " + token);  // Depuración

            switch (token.ToLower())  // Normalizamos la comparación para que no sea sensible a mayúsculas
            {
                case "if":
                    E_if();
                    break;
                case "comentario":
                    token = Leer.ReadLine();  // Lee el comentario
                    break;
                case "do":
                    E_do();
                    break;
                case "while":
                    E_while();
                    break;
                case "switch":
                    E_switch();
                    break;
                case "identificador":
                    asignacion();
                    break;
                case "function":
                    llamada_funcion();
                    break;
                default:
                    Console.WriteLine($"Token desconocido: {token}");
                    ErrorS(token, "Sentencia esperada");
                    break;
            }
        }
        private void llamada_funcion()
        {
            // Verificamos si el token siguiente es un paréntesis "("
            token = Leer.ReadLine();
            if (token == "(")
            {
                // Llamada a función, debemos leer los argumentos (si los hay)
                token = Leer.ReadLine();

                // Aquí podrías procesar los argumentos de la función
                // Dependiendo de la complejidad de la función, podrías necesitar leer múltiples tokens
                while (token != ")" && token != "fin")
                {
                    // Procesar el argumento (puedes hacer lo que sea necesario con él)
                    Console.WriteLine("Argumento de la función: " + token);
                    token = Leer.ReadLine();
                }

                if (token != ")")
                {
                    ErrorS(token, "Se esperaba un paréntesis de cierre ')'.");
                }

                // Si la llamada a función es correcta, podemos seguir con la ejecución del código
                Console.WriteLine("Función llamada correctamente.");
            }
            else
            {
                // Si no hay paréntesis después de 'function', lanzar un error
                ErrorS(token, "Se esperaba '('.");
            }
        }
        private void TestTokens()
{
    string[] tokensPrueba = { "if", "(", "condicion", ")", "{", "codigo", "}" };
    foreach (var testToken in tokensPrueba)
    {
        Console.WriteLine("Token leído: " + testToken);
        token = testToken;

        switch (token.ToLower())
        {
            case "if":
                Console.WriteLine("Es un 'if'");
                break;
            case "(":
                Console.WriteLine("Es un paréntesis abierto '('");
                break;
            case ")":
                Console.WriteLine("Es un paréntesis cerrado ')'");
                break;
            case "{":
                Console.WriteLine("Es un '{'");
                break;
            case "}":
                Console.WriteLine("Es un '}'");
                break;
            default:
                Console.WriteLine($"Token desconocido: {token}");
                break;
        }
    }
}


        private void E_switch()
        {
            // Leer la condición del switch
            token = Leer.ReadLine();
            if (token == "(")
            {
                // Procesar la condición dentro del switch (la parte que está entre los paréntesis)
                E_Condicion();  // Llamamos a E_Condicion() para manejar la condición

                // Leer el paréntesis de cierre ')'
                token = Leer.ReadLine();
                if (token != ")")
                {
                    ErrorS(token, "Se esperaba ')' para cerrar la condición del switch");
                    return;
                }

                // Leer la apertura del bloque del switch
                token = Leer.ReadLine();
                if (token != "{")
                {
                    ErrorS(token, "Se esperaba '{' para abrir el bloque del switch");
                    return;
                }

                // Leer los casos dentro del switch
                token = Leer.ReadLine();
                while (token != "}" && token != "fin")
                {
                    if (token.StartsWith("case"))
                    {
                        // Procesar un caso específico
                        ProcesarCase();
                    }
                    else if (token == "default")
                    {
                        // Procesar el caso default
                        ProcesarDefault();
                    }
                    else
                    {
                        ErrorS(token, "Se esperaba 'case' o 'default' dentro del bloque 'switch'");
                        return;
                    }

                    // Leer el siguiente token
                    token = Leer.ReadLine();
                }

                // Validamos si el bloque no fue cerrado correctamente
                if (token != "}")
                {
                    ErrorS(token, "Se esperaba '}' para cerrar el bloque del switch");
                }
            }
            else
            {
                ErrorS(token, "(");
            }
        }

        private void ProcesarCase()
        {
            // Leer el valor del case (puede ser un entero, una cadena, etc.)
            token = Leer.ReadLine();
            if (token != ":")
            {
                ErrorS(token, "Se esperaba ':' después de la condición del case");
                return;
            }

            // Procesar las sentencias dentro de este caso
            token = Leer.ReadLine();
            while (token != "break" && token != "fin" && token != "case" && token != "default")
            {
                // Si encontramos una sentencia, la procesamos (puede ser una asignación, llamada a función, etc.)
                Sentencias();
                token = Leer.ReadLine();
            }

            // Verificamos si el caso termina con un 'break'
            if (token == "break")
            {
                token = Leer.ReadLine();
                if (token != ";")
                {
                    ErrorS(token, "Se esperaba ';' después del 'break'");
                }
            }
            else
            {
                ErrorS(token, "Se esperaba 'break' al final del case");
            }
        }

        private void ProcesarDefault()
        {
            // Leer el bloque de sentencias para el caso default
            token = Leer.ReadLine();
            if (token != ":")
            {
                ErrorS(token, "Se esperaba ':' después del 'default'");
                return;
            }

            // Procesar las sentencias dentro del default
            token = Leer.ReadLine();
            while (token != "break" && token != "fin" && token != "case" && token != "default")
            {
                // Si encontramos una sentencia, la procesamos
                Sentencias();
                token = Leer.ReadLine();
            }

            // Verificamos si el default termina con un 'break'
            if (token == "break")
            {
                token = Leer.ReadLine();
                if (token != ";")
                {
                    ErrorS(token, "Se esperaba ';' después del 'break' en default");
                }
            }
            else
            {
                ErrorS(token, "Se esperaba 'break' al final del default");
            }
        }
        private void E_if()
        {
            token = Leer.ReadLine(); // Leer la condición de la sentencia if
            if (token == "(")
            {
                E_Condicion();  // Procesar la condición de la sentencia
                token = Leer.ReadLine(); // Leer el bloque de código después de la condición
                if (token == "{")
                {
                    Bloque_Sentencia(); // Leer el bloque de sentencias dentro del if
                }
                else
                {
                    ErrorS(token, "Se esperaba '{' para abrir el bloque del 'if'");
                }

                // Procesar el bloque else, si lo hay
                token = Leer.ReadLine();
                if (token == "else")
                {
                    token = Leer.ReadLine(); // Leer el bloque else
                    if (token == "{")
                    {
                        Bloque_Sentencia();  // Leer el bloque de sentencias dentro del else
                    }
                    else
                    {
                        ErrorS(token, "Se esperaba '{' para abrir el bloque 'else'");
                    }
                }
            }
            else
            {
                ErrorS(token, "(");
            }
        }

        private void E_while()
        {
            token = Leer.ReadLine(); // Leer la condición del while
            if (token == "(")
            {
                E_Condicion();  // Procesar la condición de la sentencia while
                token = Leer.ReadLine(); // Leer el bloque de código después de la condición
                if (token == "{")
                {
                    Bloque_Sentencia(); // Leer el bloque de sentencias dentro del while
                }
                else
                {
                    ErrorS(token, "Se esperaba '{' para abrir el bloque 'while'");
                }
            }
            else
            {
                ErrorS(token, "(");
            }
        }

        private void E_do()
        {
            token = Leer.ReadLine(); // Leer el bloque de sentencias dentro del do
            if (token == "{")
            {
                Bloque_Sentencia();  // Leer el bloque de sentencias dentro del do
                token = Leer.ReadLine(); // Leer la sentencia while
                if (token == "while")
                {
                    token = Leer.ReadLine(); // Leer la condición
                    if (token == "(")
                    {
                        E_Condicion(); // Procesar la condición
                        token = Leer.ReadLine(); // Leer el cierre del bloque do-while
                        if (token == ")")
                        {
                            token = Leer.ReadLine();
                            if (token != ";")
                            {
                                ErrorS(token, "Se esperaba ';' al final del do-while");
                            }
                        }
                        else
                        {
                            ErrorS(token, ")");
                        }
                    }
                    else
                    {
                        ErrorS(token, "(");
                    }
                }
                else
                {
                    ErrorS(token, "Se esperaba 'while' después de 'do'");
                }
            }
            else
            {
                ErrorS(token, "{");
            }
        }

       

        private bool EsTipoRetorno(string token)
        {
            return token == "int" || token == "void" || token == "char" || token == "float";
        }

        private bool EsIdentificador(string token)
        {
            // Aquí se puede agregar una expresión regular o verificación más avanzada
            return token.All(char.IsLetterOrDigit);
        }
        private void asignacion()
        {
            // Leer el identificador (nombre de la variable)
            token = Leer.ReadLine();
            if (token == "identificador")
            {
                // Leer el operador de asignación (=)
                token = Leer.ReadLine();
                if (token == "=")
                {
                    // Leer la expresión que se asignará (puede ser un número, una variable o una expresión)
                    token = Leer.ReadLine();
                    if (token == "numero" || token == "identificador" || token == "(")
                    {
                        // Si es un número, identificador o expresión válida, procesamos la asignación
                        ProcesarExpresion();

                        // Verificar si el final de la asignación es correcto
                        token = Leer.ReadLine();
                        if (token != ";")
                        {
                            ErrorS(token, "Se esperaba ';' al final de la asignación");
                        }
                    }
                    else
                    {
                        // Error si el valor no es una expresión válida
                        ErrorS(token, "Se esperaba una expresión válida para la asignación");
                    }
                }
                else
                {
                    // Error si no encontramos el operador de asignación
                    ErrorS(token, "Se esperaba '=' en la asignación");
                }
            }
            else
            {
                // Error si no encontramos un identificador para la asignación
                ErrorS(token, "Se esperaba un identificador para la asignación");
            }
        }
        //Expresion aritmetica
        // Validación de expresión aritmética
        private bool EsExpresionAritmetica(string expresion)
        {
            string pattern = @"^\s*-?\d+(\.\d+)?\s*([+\-*/%]\s*-?\d+(\.\d+)?)\s*$";
            return Regex.IsMatch(expresion, pattern);
        }

        // Validación de expresión lógica
        private bool EsExpresionLogica(string expresion)
        {
            string pattern = @"^\s*-?\d+(\.\d+)?\s*(==|!=|<|<=|>|>=)\s*-?\d+(\.\d+)?\s*$";
            return Regex.IsMatch(expresion, pattern);
        }

        // Manejo de asignaciones
        private void Asignacion()
        {
            token = Leer.ReadLine(); // Lee el identificador (variable)

            if (token == "identificador")  // Verifica que es un identificador válido
            {
                string variable = token; // Guarda el nombre de la variable
                token = Leer.ReadLine();  // Lee el '='

                if (token == "=")  // Verifica si la asignación tiene el operador '='
                {
                    token = Leer.ReadLine();  // Lee la parte derecha de la asignación

                    // Verificar si la parte derecha es una expresión aritmética o lógica válida
                    if (EsExpresionAritmetica(token) || EsExpresionLogica(token))
                    {
                        Rtbx_salida.AppendText($"Asignando expresión a {variable}: {token}\n");
                    }
                    else
                    {
                        ErrorS(token, "Expresión inválida"); // Reporta error si no es una expresión válida
                    }
                }
                else
                {
                    ErrorS(token, "Se esperaba '='"); // Reporta error si no se encuentra '='
                }
            }
            else
            {
                ErrorS(token, "Se esperaba un identificador para la asignación"); // Reporta error si no es un identificador válido
            }
        }



        private void ProcesarExpresion()
        {
            
            token = Leer.ReadLine();
            if (token == "numero")
            {
                
            }
            else if (token == "identificador")
            {
                
            }
            else if (token == "(")
            {
                
                token = Leer.ReadLine();
                if (token == ")")
                {
                    
                }
                else
                {
                    ErrorS(token, "Se esperaba ')' para cerrar la expresión");
                }
            }
            else
            {
                ErrorS(token, "Expresión inválida");
            }
        }



    }
}