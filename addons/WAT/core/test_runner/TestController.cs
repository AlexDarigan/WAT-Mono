﻿using System;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using Object = Godot.Object;

namespace WAT
{
    public class TestController: Node
    {
        private GDScript Case = ResourceLoader.Load<GDScript>("res://addons/WAT/core/test/case.gd");
        private GDScript Watcher = ResourceLoader.Load<GDScript>("res://addons/WAT/core/test/watcher.gd");
        private GDScript Yielder = ResourceLoader.Load<GDScript>("res://addons/WAT/core/test/yielder.gd");

        // These exist in the name space
        //const Assertions: Script = preload("res://addons/WAT/core/assertions/assertions.gd")
        //const Parameters: Script = preload("res://addons/WAT/core/test/parameters.gd")
        //const Recorder: Script = preload("res://addons/WAT/core/test/recorder.gd")
        
        // We do not use doubles in CSharp but maybe keep this around for people who like to mix
        //const Any: Script = preload("res://addons/WAT/core/test/any.gd")
        //const Director: Script = preload("res://addons/WAT/core/double/factory.gd")
        //const Registry: Script = preload("res://addons/WAT/core/double/registry.gd")
        //enum { START, PRE, EXECUTE, POST, END }
        //signal finished
            // signal done

        [Signal]
        public delegate void finished();

        [Signal]
        public delegate void done();
        
        enum STATE { START, PRE, EXECUTE, POST, END }

        private Test Test;
        private Node TestCase;
        private STATE State = STATE.START;
        private int Cursor = -1;
        private Array Methods = new Array();
        private string CurrentMethod;
        private Assertions Assertions;
        private Timer _Yielder;
        private Object _Watcher;

        public TestController()
        {
            Console.WriteLine("Mono!");
            Assertions = new Assertions();
            _Watcher = (Object) Watcher.New();
            _Yielder = (Timer) Yielder.New();
            AddChild(_Yielder);
            _Yielder.Connect("finished", this, nameof(Next));
        }

        public void Run(Dictionary test)
        {
            Console.WriteLine("Adding");
            Test = (Test) ((CSharpScript) test["script"]).New();
            Console.WriteLine("Reached");
            TestCase = (Node) Case.New(Test, test["path"]);
            Test.Assert = this.Assertions;
            Test.Watcher = this.Watcher;
            Test.Yielder = this._Yielder;
            if (test.Contains("method"))
            {
                Methods.Add(test["method"]);
            }
            else
            {
                Methods = Test.GetMethodList();
            }

            if (Methods.Count == 0)
            {
                GD.PushWarning("No Tests found in " + test["path"] + "");
                CallDeferred("Complete");
            }

            Test.Connect(nameof(Test.Described), TestCase, "_on_test_method_described");
            Assertions.Connect(nameof(Assertions.Asserted), TestCase, "_on_asserted");
            Assertions.Contains(nameof(Assertions.Asserted), Test, nameof(Test.OnLastAssertion));
            AddChild(Test);
            Start();
        }

        private void Start()
        {
            Cursor = -1;
            State = STATE.START;
            Test.Start();
            Next();
            Console.WriteLine("BEGINS");
        }

        private void Pre()
        {
            State = STATE.PRE;
            Test.Pre();
            Next();
        }

        private void Execute()
        {
            State = STATE.EXECUTE;
            CurrentMethod = NextTestMethod();
            TestCase.Call("add_method", CurrentMethod);
            Test.Call(CurrentMethod);
            Next();
        }

        private void Post()
        {
            State = STATE.POST;
            Test.Post();
            Next();
        }

        private void End()
        {
            State = STATE.END;
            Test.End();
            Next();
        }

        public string NextTestMethod()
        {
            // Implement Reruns
            Cursor += 1;
            return Methods[Cursor] as string;
        }

        public void Next()
        {
            CallDeferred("ChangeState");
        }

        private void ChangeState()
        {
            if ((bool) _Yielder.Call("is_active"))
            {
                return;
            }

            if (State == STATE.END)
            {
                Complete();
            }

            switch (State)
            {
                case STATE.START:
                    Pre();
                    break;
                case STATE.PRE:
                    Execute();
                    break;
                case STATE.EXECUTE:
                    Post();
                    break;
                case STATE.POST:
                    if (IsDone())
                    {
                        End();
                    }
                    else
                    {
                        Pre();
                    }
                    break;
                case STATE.END:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Complete()
        {
            Test.Free();
            EmitSignal(nameof(finished));
        }

        private bool IsDone()
        {
            return Cursor == Methods.Count - 1;
        }

        public Dictionary GetResults()
        {
            TestCase.Call("calculate");
            Dictionary results = TestCase.Call("to_dictionary") as Dictionary;
            TestCase.Free();
            return results;
        }
        
        
    }
}