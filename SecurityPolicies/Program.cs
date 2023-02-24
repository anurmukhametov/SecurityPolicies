using System;

namespace SecurityPolicies
{
    internal static class OperationName
    {
        public const string Read = "чтение";
        public const string Write = "запись";
        public const string Grant = "передача прав";
        public const string Matrix = "матрица доступа";
        public const string Exit = "выход";
    }

    enum Operation : ushort
    {
        Read = 1,
        Write = 2,
        Grant = 3,
        Matrix = 4
    }

    internal class AccessRights
    {
        public AccessRights(int objectId, int accessRight)
        {
            ObjectId = objectId;
            AccessRight = accessRight;
        }
        public int ObjectId { get; set; }
        public int AccessRight { get; set; }
    }

    internal class SecurityPolicies
    {
        public SecurityPolicies(int subjectId, AccessRights[] accessRights)
        {
            SubjectId = subjectId;
            _accessRights = accessRights;
        }
        private AccessRights[] _accessRights;
        public int SubjectId { get; set; }
        public AccessRights this[int index]
        {
            get { return _accessRights[index]; }
            set { _accessRights[index] = value; }
        }
    }

    internal class Program
    {
        // Массив субъектов, первым субъектом всегда является Администратор
        private static string[] subjects =
        {
            "Администратор",
            "Гость №1",
            "Гость №2",
            "Гость №3",
            "Пользователь №1",
            "Пользователь №2",
            "Пользователь №3"
        };

        // Массив объектов
        private static string[] objects =
        {
            "Файл №1",
            "Файл №2",
            "Файл №3",
            "Файл №4",
            "Файл №5",
            "Файл №6"
        };

        // Массив прав доступа, первый уровень достпа всегда является минимальным, последний - максимальным
        private static string[] accesses =
        {
            "Полный запрет", // 000
            "Передача прав", // 001
            "Запись", // 010
            "Запись, Передача прав", // 011
            "Чтение", // 100
            "Чтение, Передача прав", // 101
            "Чтение, Запись", // 110
            "Полный доступ" // 111
        };

        private static SecurityPolicies[] InitializingSecurityPolicies(string[] subjects, string[] objects, string[] accesses)
        {
            var securityPolicies = new SecurityPolicies[subjects.Length];
            var random = new Random();
            for (int i = 0; i < subjects.Length; i++)
            {
                var accessRights = new AccessRights[objects.Length];
                for (int j = 0; j < objects.Length; j++)
                {
                    accessRights[j] = new AccessRights(j, i == 0 ? accesses.Length - 1 : random.Next(0, accesses.Length - 1));
                }
                securityPolicies[i] = new SecurityPolicies(i, accessRights);
            }
            return securityPolicies;
        }

        private static void PrintAccessMatrix(SecurityPolicies[] securityPolicies, int columnWidth)
        {
            string pattern = "{0," + $"{columnWidth}" + "} |";

            // Шапка таблицы
            Console.Write(pattern, "Субъект\\Объект");
            for (int i = 0; i < objects.Length; i++)
            {
                Console.Write(pattern, objects[i]);
            }
            Console.WriteLine();
            for (int i = 1; i < (columnWidth + 2) * (objects.Length + 1); i++)
            {
                Console.Write("=");
            }
            Console.WriteLine();

            // Содержимое таблицы
            for (int i = 0; i < subjects.Length; i++)
            {
                Console.Write(pattern, subjects[i]);
                for (int j = 0; j < objects.Length; j++)
                {
                    Console.Write(pattern, accesses[securityPolicies[i][j].AccessRight]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static int FindSubjectByName(string name)
        {
            int subjectId = -1;
            for (int i = 0; i < subjects.Length; i++)
            {
                if (name == subjects[i])
                {
                    subjectId = i;
                    break;
                }
            }
            return subjectId;
        }

        private static void GetAccessRights(SecurityPolicies[] securityPolicies, int subjectId)
        {
            Console.WriteLine("Перечень Ваших прав:");
            for (int i = 0; i < objects.Length; i++)
            {
                Console.WriteLine($"{objects[i]}: {accesses[securityPolicies[subjectId][i].AccessRight]}");
            }
            Console.WriteLine();
        }

        private static string GetAccessCode(SecurityPolicies[] securityPolicies, int subjectId, int objectId)
        {
            return string.Format("{0:000}", Convert.ToSByte(Convert.ToString(securityPolicies[subjectId][objectId].AccessRight, 2)));
        }

        private static bool AccessVerification(SecurityPolicies[] securityPolicies, int subjectId, int objectId, Operation operation)
        {
            var access = GetAccessCode(securityPolicies, subjectId, objectId);
            switch (operation)
            {
                case Operation.Read:
                    return access[0] == '1';
                case Operation.Write:
                    return access[1] == '1';
                case Operation.Grant:
                    return access[2] == '1';
                default:
                    return false;
            }
        }

        private static int ReadObjectNumber()
        {
            while (true)
            {
                var input = Convert.ToInt32(Console.ReadLine());
                if (input > 0 && input <= objects.Length)
                {
                    return input;
                }
                else
                {
                    Console.Write("Объекта с таким номером не существует, попробуйте еще раз: ");
                }
            }
        }

        private static void Read(SecurityPolicies[] securityPolicies, int subjectId)
        {
            Console.Write("Введите номер объекта, который вы хотите прочитать: ");
            int objectId = ReadObjectNumber() - 1;
            if (AccessVerification(securityPolicies, subjectId, objectId, Operation.Read))
            {
                Console.WriteLine("Операция прошла успешно.\n");
            }
            else
            {
                Console.WriteLine("Отказ в выполнении операции. У Вас нет прав для ее осуществления.\n");
            }
        }

        private static void Write(SecurityPolicies[] securityPolicies, int subjectId)
        {
            Console.Write("Введите номер объекта, который вы хотите записать: ");
            int objectId = ReadObjectNumber() - 1;
            if (AccessVerification(securityPolicies, subjectId, objectId, Operation.Write))
            {
                Console.WriteLine("Операция прошла успешно.\n");
            }
            else
            {
                Console.WriteLine("Отказ в выполнении операции. У Вас нет прав для ее осуществления.\n");
            }
        }

        private static void Grant(SecurityPolicies[] securityPolicies, int subjectId)
        {
            Console.Write("Введите номер объекта, права на который вы хотите передать: ");
            int objectId = ReadObjectNumber() - 1;
            if (AccessVerification(securityPolicies, subjectId, objectId, Operation.Grant))
            {
                Console.Write("Введите операцию, права на котороую вы хотите передать: ");
                var operationName = Console.ReadLine();
                switch (operationName)
                {
                    case OperationName.Read:
                        GrantRead(securityPolicies, subjectId, objectId);
                        break;
                    case OperationName.Write:
                        GrantWrite(securityPolicies, subjectId, objectId);
                        break;
                    case OperationName.Grant:
                        Console.WriteLine("Политика безопаности запрещает передавать право на данную операцию.\n");
                        break;
                    default:
                        Console.WriteLine("Неизвестная операция.\n");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Отказ в выполнении операции. У Вас нет прав для ее осуществления.");
            }
        }

        private static void GrantRead(SecurityPolicies[] securityPolicies, int subjectId, int objectId)
        {
            if (AccessVerification(securityPolicies, subjectId, objectId, Operation.Read))
            {
                Console.Write($"Введите имя пользователя, которому хотите передать права на чтение {objects[objectId]}: ");
                subjectId = FindSubjectByName(Console.ReadLine());
                if (subjectId != -1)
                {
                    string accessCode = GetAccessCode(securityPolicies, subjectId, objectId);
                    var access = accessCode.ToCharArray();
                    access[0] = '1';
                    securityPolicies[subjectId][objectId].AccessRight = Convert.ToInt32(new string(access), 2);
                    Console.WriteLine("Операция прошла успешно.\n");
                }
                else
                {
                    Console.WriteLine("Неверное имя пользователя.\n");
                }
            }
            else
            {
                Console.WriteLine($"Отказ в выполнении операции. У Вас нет права на чтение {objects[objectId]}.");
            }
        }

        private static void GrantWrite(SecurityPolicies[] securityPolicies, int subjectId, int objectId)
        {
            if (AccessVerification(securityPolicies, subjectId, objectId, Operation.Write))
            {
                Console.Write($"Введите имя пользователя, которому хотите передать права на запись {objects[objectId]}: ");
                subjectId = FindSubjectByName(Console.ReadLine());
                if (subjectId != -1)
                {
                    string accessCode = GetAccessCode(securityPolicies, subjectId, objectId);
                    var access = accessCode.ToCharArray();
                    access[1] = '1';
                    securityPolicies[subjectId][objectId].AccessRight = Convert.ToInt32(new string(access), 2);
                    Console.WriteLine("Операция прошла успешно.\n");
                }
                else
                {
                    Console.WriteLine("Неверное имя пользователя.\n");
                }
            }
            else
            {
                Console.WriteLine($"Отказ в выполнении операции. У Вас нет права на запись {objects[objectId]}.");
            }
        }

        static void Main(string[] args)
        {
            var securityPolicies = InitializingSecurityPolicies(subjects, objects, accesses);
            var input = string.Empty;

            while (input != OperationName.Exit)
            {
                int subjectId = -1;
                Console.Write("Введите имя пользователя: ");
                while (subjectId == -1)
                {
                    input = Console.ReadLine();
                    if (input != OperationName.Exit)
                    {
                        subjectId = FindSubjectByName(input);
                        if (subjectId != -1)
                        {
                            Console.WriteLine("Идентификация прошла успешно, добро пожаловать в систему!\n");
                        }
                        else
                        {
                            Console.Write("Неверное имя пользователя, попробуйте еще раз: ");
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (subjectId != -1)
                {
                    GetAccessRights(securityPolicies, subjectId);

                    while (input != OperationName.Exit)
                    {
                        Console.Write("Введите операцию: ");
                        input = Console.ReadLine();
                        switch (input)
                        {
                            case OperationName.Read:
                                Read(securityPolicies, subjectId);
                                break;
                            case OperationName.Write:
                                Write(securityPolicies, subjectId);
                                break;
                            case OperationName.Grant:
                                Grant(securityPolicies, subjectId);
                                break;
                            case OperationName.Matrix:
                                PrintAccessMatrix(securityPolicies, 23);
                                break;
                            case OperationName.Exit:
                                Console.WriteLine();
                                break;
                            default:
                                Console.WriteLine("Неизвестная операция.\n");
                                break;
                        }
                    }
                    input = string.Empty;
                }
            }
        }
    }
}
