﻿using System;
using System.Linq;
using NLua;

namespace XPlat.LuaScripting
{
    public class LuaHost
    {
        private Lua state;

        public LuaHost()
        {
            this.state = new Lua();
            state.LoadCLRPackage();
        }

        public LuaScript CreateScript(string script = null)
        {
            return new LuaScript(state, script);
        }

        public void SetGlobal(string name, object obj){
            state[name] = obj;
        }

        public void ImportNamespace(string ns)
        {
            state.DoString($"import ('{ns}')");
        }

        public LuaTable? ParseTable(string args)
        {
            return state.DoString($"return {args}").FirstOrDefault() as LuaTable;
        }
    }
}

