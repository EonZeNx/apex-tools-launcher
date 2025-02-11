# Apex Tools Launcher
Mod tools for [Avalanche Studios Group](https://avalanchestudios.com/) games and [Apex Engine](https://avalanchestudios.com/technology) files. Launch, extract and repack their games and files.

## File type status
|     File type    | Version | Extract | Repack |
| ---------------- | ------- | ------- | ------ |
| Inline Container | -       | X       | ~      |
| RTPC             | 1       | X       | ~      |
| RTPC             | 3       | X       | ~      |
| AAF              | 1       | X       |        |
| SARC             | 2       | X       |        |
| SARC             | 3       | X       |        |
| TOC              | -       | X       |        |
| ADF              | 4       | X       |        |
| TAB / ARC        | 2       | X       |        |
| DDSC / AVTX      | 1       |         |        |

# Installation
1. Install the latest Dot Net 9 runtime.
2. Download the latest release and extract the zip file.
3. Run the CLI or GUI.

# Structure
The project is intended to be readable and easy to maintain. To support this, close locality of behaviour and data is upheld.

Data is stored in classes with logic separated using extensions. Class members are values stored in binary files while properties are data otherwise not directly stored within the structure, e.g. `NameIndex` member stored within the binary struct vs `Name` property which has been looked up.

# References
- **[aaronkirkham's jc-model-renderer](https://github.com/aaronkirkham)**: The JC Model Renderer had some very useful info, especially regarding compression.
- **[PredatorCZ's ApexLib](https://github.com/PredatorCZ/ApexLib)**: Lots of model format info here.
- **[kk19's DECA](https://github.com/kk49/deca)**: Very helpful for detailed file formats.

### Communities
Join the largest Just Cause community here: [Just Cause Unlimited](https://discord.gg/just-cause-unlimited-449584016648044555)

Join the EonZeNx server here: [EonZeNx Discord](https://discord.gg/SAjVFmMGdd)

# License
All information is under the [MIT license](https://choosealicense.com/licenses/mit/)

### Note
*Never include binaries or data that do not adhere to the above license.*

## Disclaimer
All product names, logos, and brands are property of their respective owners. All company, product and service names
used in this website are for identification purposes only. Use of these names, logos, and brands does not imply endorsement.
