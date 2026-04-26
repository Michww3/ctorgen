# ctorgen

> CLI tool for generating ladder-style constructors for C# classes

`ctorgen` eliminates repetitive constructor boilerplate by generating a full chain of overloaded constructors where parameters are progressively replaced with their hash representations.

---

## 📚 Table of Contents

- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [🚀 Quick Start](#-quick-start)
- [🖥 CLI Usage](#-cli-usage)
- [⚙️ Configuration](#️-configuration)
- [🧬 Generated Output](#-generated-output)
- [🧠 How It Works](#-how-it-works)
- [📊 Before / After](#-before--after)
- [📋 Arguments](#-arguments)
- [📁 Examples](#-examples)
- [📄 License](#-license)
- [🤝 Contributing](#-contributing)

---

## ✨ Features

- Generates **all constructor overloads automatically**
- Implements **ladder-style chaining** via `this(...)`
- Seamlessly transitions from **values → hashes**
- Supports both **CLI** and **JSON configuration**
- Deterministic and consistent output
- Zero boilerplate for hash-driven models

---

## 📦 Installation

### Global tool

```bash
dotnet tool install --global ctorgen
```

### Local build (run from src folder)

```bash
dotnet pack -c Release
dotnet tool install --global --add-source bin/Release ctorgen
```

---

## 🚀 Quick Start

### 1. Create config

```bash
ctorgen init
```

Creates:

```
ctorgen.json
```

---

### 2. Generate constructors

```bash
ctorgen --config=ctorgen.json
```

Output:

```
MyModelHash.Ctors.cs
```

---

## 🖥 CLI Usage

```bash
ctorgen --name=MyModelHash --param="id:IGuid:new DeterminedHash(id)" --param="name:IString:new DeterminedHash(name)" --out=MyModelHash.Ctors.cs
```

---

## ⚠️ Parameter Format

```
name:type:hashExpr
```

Example:

```bash
--param="id:IGuid:new DeterminedHash(id)"
```

---

### ❗ Important: quoting

If your expression contains spaces, wrap it in quotes:

✅ Correct:
```bash
--param="id:IGuid:new DeterminedHash(id)"
```

❌ Incorrect:
```bash
--param=id:IGuid:new DeterminedHash(id)
```

---

## ⚙️ Configuration

Example `ctorgen.json`:

```json
{
  "ClassName": "MyModelHash",
  "Params": [
    {
      "Name": "id",
      "Type": "IGuid",
      "Hash": "new DeterminedHash(id)"
    },
    {
      "Name": "name",
      "Type": "IString",
      "Hash": "new DeterminedHash(name)"
    }
  ]
}
```

---

## 🧬 Generated Output

### Delegating constructor

```csharp
public MyModelHash(
    IGuid id,
    IString name
)
    : this(
        new DeterminedHash(id),
        name
    )
{ }
```

---

### Final constructor

```csharp
public MyModelHash(
    IDeterminedHash idHash,
    IDeterminedHash nameHash
)
{
    _idHash = idHash;
    _nameHash = nameHash;
}
```

---

## 🧠 How It Works

For a model with **N parameters**, ctorgen:

1. Generates all combinations using a **bitmask**
2. Sorts them by number of hashed parameters
3. Builds a **constructor chain**
4. Each step converts one value → hash
5. Final constructor assigns fields

Result:

```
values → partial hashes → full hashes
```

---

## 📊 Before / After

### ❌ Manual

- Dozens of constructor overloads
- Easy to make mistakes
- Hard to maintain

### ✅ ctorgen

```bash
ctorgen --config=ctorgen.json
```

- All combinations generated automatically
- Clean and consistent
- Minimal maintenance

---

## 📋 Arguments

| Argument   | Description |
|------------|------------|
| `--name`   | Target class name |
| `--param`  | Parameter (`name:type:hashExpr`) |
| `--config` | JSON config path |
| `--out`    | Output file (default `{ClassName}.Ctors.cs`) |
| `init`     | Generate config template |
| `--help`   | Show help |

---

## 📁 Examples

```
examples/
├── ctorgen.json
└── MyModelHash.Ctors.cs
```

---

## 📄 License

MIT

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome.
