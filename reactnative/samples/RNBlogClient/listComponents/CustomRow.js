import React from 'react';
import { View, Text, StyleSheet, Image } from 'react-native';
import Icon from 'react-native-vector-icons/FontAwesome';

const styles = StyleSheet.create({
    container: {
        flex: 1,
        flexDirection: 'row',
        padding: 10,
        marginLeft:10,
        marginRight:10,
        marginTop: 8,
        marginBottom: 8,
        elevation: 2,
    },
    title: {
        fontSize: 16,
        color: 'darkred',
        fontWeight: 'bold'
    },
    time: {
        fontSize: 16,
        color: 'gray',
    },
    container_text: {
        flex: 1,
        flexDirection: 'column',
        marginLeft: 12,
        justifyContent: 'center',
    },
    description: {
        fontSize: 16,
    },
    photo: {
        height: 50,
        width: 50,
        borderRadius: 10,
        marginTop: 5
    },
});

const CustomRow = ({ title, time, description, image_url }) => (
    <View style={styles.container}>
        <Image source={{ uri: image_url }} style={styles.photo} />
        <View style={styles.container_text}>
            <Text style={styles.title}>
                {title.toUpperCase()}
            </Text>
            <Text style={styles.description}  ellipsizeMode='tail' numberOfLines={2}>
                {description}
            </Text>
            <Text style={styles.time}>
                <Icon name="clock-o"/>{" " +time}
            </Text>
        </View>

    </View>
);

export default CustomRow;