# AoC23

Advent of Code challenge 2023

## Template Usage

### Installing the Template

```
cd <path to AoCTemplate>
dotnet new install AoCTemplate --force
```

### Create a New Project

Example to create a new project for day 1:

```
day=01 && dotnet new AoC -o $day -D $day -R int && dotnet sln add $day
```

For a different year than 2023, use the `-Y` option, or edit the default value for the Year parameter in the template.json file.

Use `dotnet new AoC --help` for more information.

see https://github.com/dotnet/templating/wiki/Reference-for-template.json