import React from 'react';
import { View, ListView, FlatList, StyleSheet, Text } from 'react-native';
import CustomRow from './CustomRow';
import { useTheme } from '@react-navigation/native';

const styles = StyleSheet.create({
    container: {
        flex: 1,
    },
});


const CustomList = ({ itemList }) => (
        <FlatList
                data={itemList}
                renderItem={({ item }) => <CustomRow
                key={item.key}
                    title={item.title}
                    time={item.time}
                    description={item.description}
                    image_url={item.image_url}
                />}
            />
);

export default CustomList;